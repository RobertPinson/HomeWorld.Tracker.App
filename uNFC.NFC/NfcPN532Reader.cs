using System;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Hardware.Nfc;

namespace uPLibrary.Nfc
{
    /// <summary>
    /// NFC reader class for NXP PN532 ic
    /// </summary>
    public class NfcPN532Reader : INfcReader
    {
        // reference to NFC PN532 ic
        private PN532 pn532;

        // thread for listening tag detection
        //private Thread detectionThread;
        private bool isRunning;

        // current PN532 target type listening (baud rate/protocol)
        private PN532.TargetType targetType;

        // current NFC tag type
        private NfcTagType nfcTagType;
        // current NFC connection to a tag
        private NfcTagConnection nfcTagConn;       
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commLayer">Communication layer to use with PN532 ic</param>
        public NfcPN532Reader(IPN532CommunicationLayer commLayer)
        {
            pn532 = new PN532(commLayer);
        }

        #region INfcReader interface ...

        public event TagEventHandler TagDetected;
        public event TagEventHandler TagLost;

        public async Task Open(NfcTagType nfcTagType)
        {
            this.nfcTagType = nfcTagType;
            // map NFC tag type to PN532 target type (baud rate/protocol)
            targetType = NfcToPN532Type(nfcTagType);

            isRunning = true;
            nfcTagConn = null;

            // get info and configure PN532 ic
            await pn532.GetFirmwareVersion();
            await pn532.SAMConfiguration(PN532.SamMode.NormalMode, false);

            // start thread for tag detection
           await Task.Factory.StartNew(DetectionThread,
               CancellationToken.None,
               TaskCreationOptions.LongRunning,
               TaskScheduler.Default);
        }

        public void Close()
        {
            pn532.Close();
            nfcTagConn = null;
            isRunning = false;
        }

        public async Task<byte[]> WriteRead(byte[] data)
        {
            byte[] output = await pn532.InDataExchange(1, data);

            if (output == null)
                return null;
            
            byte[] dataIn = new byte[output.Length - 1]; // -1 -> remove command code response
            Array.Copy(output, 1, dataIn, 0, dataIn.Length);
            return dataIn;
        }

        #endregion

        /// <summary>
        /// Map NFC tag type to PN532 target type (baud rate/protocol)
        /// </summary>
        /// <param name="nfcTagType">NFC target type</param>
        /// <returns>Target type (baud rate)</returns>
        private PN532.TargetType NfcToPN532Type(NfcTagType nfcTagType)
        {
            switch (nfcTagType)
            {
                case NfcTagType.MifareClassic4k:
                case NfcTagType.MifareUltralight:
                    return PN532.TargetType.Iso14443TypeA;
                default:
                    return PN532.TargetType.Iso14443TypeA;
            }
        }
        
        /// <summary>
        /// Raise tag detected event
        /// </summary>
        /// <param name="nfcTagType">NFC tag type</param>
        /// <param name="conn">Connection instance to NFC tag</param>
        private void OnTagDetected(NfcTagType nfcTagType, NfcTagConnection conn)
        {
            if (TagDetected != null)
                TagDetected(this, new NfcTagEventArgs(nfcTagType, conn));
        }

        /// <summary>
        /// Raise tag lost event
        /// </summary>
        /// <param name="nfcTagType">NFC tag type</param>
        /// <param name="conn">Connection instance to NFC tag</param>
        private void OnTagLost(NfcTagType nfcTagType, NfcTagConnection conn)
        {
            if (TagLost != null)
                TagLost(this, new NfcTagEventArgs(nfcTagType, conn));
        }

        /// <summary>
        /// Thread for tag detecting
        /// </summary>
        private async Task DetectionThread()
        {
            while (isRunning)
            {
                byte[] target = await pn532.InListPassiveTarget(targetType);

                // target detected
                if (target != null)
                {
                    switch (targetType)
                    {
                        case PN532.TargetType.Iso14443TypeA:

                            // no current tag set
                            if (nfcTagConn == null)
                            {
                                // get ATQA and SAK
                                byte[] ATQA = new byte[2];
                                Array.Copy(target, PN532.ISO14443A_SENS_RES_OFFSET + 2, ATQA, 0, ATQA.Length); // +2 for 4B and NbTg (pn532um.pdf, pag. 116)
                                byte SAK = target[PN532.ISO14443A_SEL_RES_OFFSET + 2]; // +2 for 4B and NbTg (pn532um.pdf, pag. 116)

                                // get tag ID
                                byte[] tagId = new byte[target[PN532.ISO14443A_IDLEN_OFFSET + 2]]; // +2 for 4B and NbTg (pn532um.pdf, pag. 116)
                                Array.Copy(target, PN532.ISO14443A_IDLEN_OFFSET + 2 + 1, tagId, 0, tagId.Length);

                                // check ATQA and SAK for Mifare Classic 1k
                                if (ATQA[0] == 0x00 && ATQA[1] == 0x04 && SAK == 0x08)
                                {
                                    nfcTagType = NfcTagType.MifareClassic1k;
                                    nfcTagConn = new NfcMifareTagConnection(this, tagId);
                                    OnTagDetected(nfcTagType, nfcTagConn);
                                }
                                // check ATQA and SAK for Mifare Ultralight
                                else if (ATQA[0] == 0x00 && ATQA[1] == 0x44 && SAK == 0x00)
                                {
                                    nfcTagType = NfcTagType.MifareUltralight;
                                    nfcTagConn = new NfcMifareUlTagConnection(this, tagId);
                                    OnTagDetected(nfcTagType, nfcTagConn);
                                }
                            }
                            break;

                        default: 
                            break;
                    }
                }
                // no target detected
                else
                {
                    // if current tag set and no target detected...
                    if (nfcTagConn != null)
                    {
                        // ...current tag is lost
                        OnTagLost(nfcTagType, nfcTagConn);
                        nfcTagConn = null;
                    }
                }
            }
        }       
    }

    /// <summary>
    /// NFC tag type
    /// </summary>
    public enum NfcTagType
    {
        /// <summary>
        /// Mifare Classic 1 KB
        /// </summary>
        MifareClassic1k,

        /// <summary>
        /// Mifare Classic 4 KB
        /// </summary>
        MifareClassic4k,

        /// <summary>
        /// Mifare Ultralight
        /// </summary>
        MifareUltralight
    }
}

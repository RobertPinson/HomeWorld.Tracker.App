using System;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Hardware.Nfc;

namespace uPLibrary.Nfc
{
    /// <summary>
    /// NFC reader class for NXP PN532 ic
    /// </summary>
    public class NfcPn532Reader : INfcReader
    {
        // reference to NFC PN532 ic
        private readonly PN532 _pn532;

        private bool _isRunning;

        // current PN532 target type listening (baud rate/protocol)
        private PN532.TargetType _targetType;

        // current NFC tag type
        private NfcTagType _nfcTagType;
        // current NFC connection to a tag
        private NfcTagConnection _nfcTagConn;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commLayer">Communication layer to use with PN532 ic</param>
        public NfcPn532Reader(IPN532CommunicationLayer commLayer)
        {
            _pn532 = new PN532(commLayer);
        }

        #region INfcReader interface ...

        public event TagEventHandler TagDetected;
        public event TagEventHandler TagLost;

        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public async Task Open(NfcTagType nfcTagType)
        {
            this._nfcTagType = nfcTagType;
            // map NFC tag type to PN532 target type (baud rate/protocol)
            _targetType = NfcToPn532Type(nfcTagType);

            _nfcTagConn = null;

            // get info and configure PN532 ic
            await _pn532.GetFirmwareVersion();
            await _pn532.SAMConfiguration(PN532.SamMode.NormalMode, false);

            _isRunning = true;

            // start thread for tag detection
            await Task.Factory.StartNew(DetectionThread,
               CancellationToken.None,
               TaskCreationOptions.LongRunning,
               TaskScheduler.Default);
        }

        public void Close()
        {
            _pn532.Close();
            _nfcTagConn = null;
            _isRunning = false;
        }

        public async Task<byte[]> WriteRead(byte[] data)
        {
            byte[] output = await _pn532.InDataExchange(1, data);

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
        private PN532.TargetType NfcToPn532Type(NfcTagType nfcTagType)
        {
            switch (nfcTagType)
            {
                case NfcTagType.MifareClassic4K:
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
            while (_isRunning)
            {
                try
                {
                    var target = await _pn532.InListPassiveTarget(_targetType);

                    // target detected
                    if (target != null)
                    {
                        switch (_targetType)
                        {
                            case PN532.TargetType.Iso14443TypeA:

                                // no current tag set
                                if (_nfcTagConn == null)
                                {
                                    // get ATQA and SAK
                                    byte[] atqa = new byte[2];
                                    Array.Copy(target, PN532.ISO14443A_SENS_RES_OFFSET + 2, atqa, 0, atqa.Length); // +2 for 4B and NbTg (pn532um.pdf, pag. 116)
                                    byte sak = target[PN532.ISO14443A_SEL_RES_OFFSET + 2]; // +2 for 4B and NbTg (pn532um.pdf, pag. 116)

                                    // get tag ID
                                    byte[] tagId = new byte[target[PN532.ISO14443A_IDLEN_OFFSET + 2]]; // +2 for 4B and NbTg (pn532um.pdf, pag. 116)
                                    Array.Copy(target, PN532.ISO14443A_IDLEN_OFFSET + 2 + 1, tagId, 0, tagId.Length);

                                    // check ATQA and SAK for Mifare Classic 1k
                                    if (atqa[0] == 0x00 && atqa[1] == 0x04 && sak == 0x08)
                                    {
                                        _nfcTagType = NfcTagType.MifareClassic1K;
                                        _nfcTagConn = new NfcMifareTagConnection(this, tagId);
                                        OnTagDetected(_nfcTagType, _nfcTagConn);
                                    }
                                    // check ATQA and SAK for Mifare Ultralight
                                    else if (atqa[0] == 0x00 && atqa[1] == 0x44 && sak == 0x00)
                                    {
                                        _nfcTagType = NfcTagType.MifareUltralight;
                                        _nfcTagConn = new NfcMifareUlTagConnection(this, tagId);
                                        OnTagDetected(_nfcTagType, _nfcTagConn);
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
                        if (_nfcTagConn != null)
                        {
                            // ...current tag is lost
                            OnTagLost(_nfcTagType, _nfcTagConn);
                            _nfcTagConn = null;
                        }
                    }
                }
                catch (Exception)
                {
                    //TODO LOG ex
                    _isRunning = false;
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
        MifareClassic1K,

        /// <summary>
        /// Mifare Classic 4 KB
        /// </summary>
        MifareClassic4K,

        /// <summary>
        /// Mifare Ultralight
        /// </summary>
        MifareUltralight
    }
}

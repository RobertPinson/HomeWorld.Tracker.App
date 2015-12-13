using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace uPLibrary.Hardware.Nfc
{
    /// <summary>
    /// HSU communication layer
    /// </summary>
    public class Pn532CommunicationHsu : IPN532CommunicationLayer
    {
        // default timeout to wait PN532
        private const int WaitTimeout = 500;

        #region HSU Constants ...

        private const int HsuBaudRate = 115200;
        private const SerialParity HsuParity = SerialParity.None;
        private const int HsuDataBits = 8;
        private const SerialStopBitCount HsuStopBits = SerialStopBitCount.One;

        #endregion

        // hsu interface
        private readonly SerialDevice _rfidReader;
        // event on received frame
        private readonly AutoResetEvent _received;

        // frame parser state
        private HsuParserState _state;
        private bool _isFirstStartCode;
        private bool _isWaitingAck;
        // frame buffer
        private readonly IList _frame;
        private int _length;
        // internal buffer from serial port
        private readonly IList _inputBuffer;

        // frames queue received
        private readonly Queue _queueFrame;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceName">Serial port name</param>
        private Pn532CommunicationHsu(SerialDevice reader)
        {
            if (reader == null) throw new ArgumentNullException("serial card reader is not set");

            _length = 0;
            _isFirstStartCode = false;
            _isWaitingAck = false;

            _frame = new ArrayList();
            _inputBuffer = new ArrayList();
            _queueFrame = new Queue();

            _state = HsuParserState.Preamble;
            _received = new AutoResetEvent(false);

            _rfidReader = reader;
        }

        public static async Task<Pn532CommunicationHsu> CreateSerialPort(string deviceName)
        {
            string deviceQuery = SerialDevice.GetDeviceSelector();
            var discovered = await DeviceInformation.FindAllAsync(deviceQuery);
            var readerInfo = discovered.FirstOrDefault(x => x.Name.IndexOf(deviceName, StringComparison.OrdinalIgnoreCase) > 0);
            //var readerInfo = discovered[0];

            SerialDevice reader = null;

            if (readerInfo != null)
            {

                reader = await SerialDevice.FromIdAsync(readerInfo.Id);

                if (reader != null)
                {
                    reader.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                    reader.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    reader.BaudRate = 115200;
                    reader.Parity = SerialParity.None;
                    reader.StopBits = SerialStopBitCount.One;
                    reader.DataBits = 8;
                }
                else
                {
                    throw new Exception("Reader Not configured");
                }
            }

            return new Pn532CommunicationHsu(reader);
        }

        /// <summary>
        /// internal encapsulation of the response read from the Cottonwood
        /// after issuing a command
        /// </summary>
        internal class RfidReaderResult
        {
            public bool IsSuccessful { get; set; }
            public byte[] Result { get; set; }

        }

        /// <summary>
        /// Serial read
        /// </summary>
        /// <returns>bytes read</returns>
        private async Task<RfidReaderResult> Read()
        {
            var retvalue = new RfidReaderResult();
            var dataReader = new DataReader(_rfidReader.InputStream);

            try
            {
                //Awaiting Data from RFID Reader
                var numBytesRecvd = await dataReader.LoadAsync(1024);
                retvalue.Result = new byte[numBytesRecvd];
                if (numBytesRecvd > 0)
                {
                    //Data successfully read from RFID Reader"
                    dataReader.ReadBytes(retvalue.Result);
                    retvalue.IsSuccessful = true;

                    Debug.WriteLine("Read: {0}", BitConverter.ToString(retvalue.Result));
                }
            }
            catch (Exception)
            {
                retvalue.IsSuccessful = false;
                throw;
            }
            finally
            {
                dataReader.DetachStream();
            }

            return retvalue;
        }

        /// <summary>
        /// Serial write function to PN532
        /// </summary>
        /// <param name="writeBytes">byte array sent to the Cottonwood</param>
        /// <returns>bytes written and success indicator</returns>
        private async Task<RfidReaderResult> Write(byte[] writeBytes)
        {
            Debug.WriteLine(string.Format("Write: {0}", BitConverter.ToString(writeBytes)));

            var dataWriter = new DataWriter(_rfidReader.OutputStream);
            var retvalue = new RfidReaderResult();

            try
            {
                //send the message
                //Writing command to RFID Reader
                dataWriter.WriteBytes(writeBytes);
                await dataWriter.StoreAsync();
                retvalue.IsSuccessful = true;
                retvalue.Result = writeBytes;
                //Writing of command has been successful
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dataWriter.DetachStream();
            }
            return retvalue;
        }

        //void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    lock (inputBuffer)
        //    {
        //        // read bytes from serial port and load them in the internal buffer
        //        byte[] buffer = new byte[port.BytesToRead];
        //        port.Read(buffer, 0, buffer.Length);

        //        for (int i = 0; i < buffer.Length; i++)
        //            inputBuffer.Add(buffer[i]);
        //    }

        //    // frame parsing
        //    ExtractFrame();
        //}

        void ExtractFrame()
        {
            lock (_inputBuffer)
            {
                foreach (byte byteRx in _inputBuffer)
                {
                    switch (_state)
                    {
                        case HsuParserState.Preamble:

                            // preamble arrived, frame started
                            if (byteRx == PN532.PN532_PREAMBLE)
                            {
                                _length = 0;
                                _isFirstStartCode = false;
                                _frame.Clear();

                                _frame.Add(byteRx);
                                _state = HsuParserState.StartCode;
                            }
                            break;

                        case HsuParserState.StartCode:

                            // first start code byte not received yet
                            if (!_isFirstStartCode)
                            {
                                if (byteRx == PN532.PN532_STARTCODE_1)
                                {
                                    _frame.Add(byteRx);
                                    _isFirstStartCode = true;
                                }
                            }
                            // first start code byte already received
                            else
                            {
                                if (byteRx == PN532.PN532_STARTCODE_2)
                                {
                                    _frame.Add(byteRx);
                                    _state = HsuParserState.Length;
                                }
                            }
                            break;

                        case HsuParserState.Length:

                            // not waiting ack, the byte is LEN
                            if (!_isWaitingAck)
                            {
                                // save data length (TFI + PD0...PDn) for counting received data
                                _length = byteRx;
                                _frame.Add(byteRx);
                                _state = HsuParserState.LengthChecksum;
                            }
                            // waiting ack, the byte is first of ack/nack code
                            else
                            {
                                _frame.Add(byteRx);
                                _state = HsuParserState.AckCode;
                            }
                            break;

                        case HsuParserState.LengthChecksum:

                            // arrived LCS
                            _frame.Add(byteRx);
                            _state = HsuParserState.FrameIdentifierAndData;
                            break;

                        case HsuParserState.FrameIdentifierAndData:

                            _frame.Add(byteRx);
                            // count received data bytes (TFI + PD0...PDn)
                            _length--;

                            // all data bytes received
                            if (_length == 0)
                                _state = HsuParserState.DataChecksum;
                            break;

                        case HsuParserState.DataChecksum:

                            // arrived DCS
                            _frame.Add(byteRx);
                            _state = HsuParserState.Postamble;
                            break;

                        case HsuParserState.Postamble:

                            // postamble received, frame end
                            if (byteRx == PN532.PN532_POSTAMBLE)
                            {
                                _frame.Add(byteRx);
                                _state = HsuParserState.Preamble;

                                // enqueue received frame
                                byte[] frameReceived = new byte[_frame.Count];
                                _frame.CopyTo(frameReceived, 0);
                                _queueFrame.Enqueue(frameReceived);

                                _received.Set();
                            }
                            break;

                        case HsuParserState.AckCode:

                            // second byte of ack/nack code
                            _frame.Add(byteRx);
                            _state = HsuParserState.Postamble;

                            _isWaitingAck = false;
                            break;

                        default:
                            break;
                    }
                }

                // clear internal buffer
                _inputBuffer.Clear();
            }
        }

        #region IPN532CommunicationLayer interface ...

        public async Task<bool> SendNormalFrame(byte[] frame)
        {
            _isWaitingAck = true;
         
            // send frame...
            var writeResult = await Write(frame);

            // wait for ack/nack
            if (writeResult.IsSuccessful)
            {
                //get response
                var readResult = await Read();

                if (readResult.IsSuccessful)
                {
                    //get received data
                    lock (_inputBuffer)
                    {
                        foreach (var t in readResult.Result)
                            _inputBuffer.Add(t);
                    }

                    // frame parsing
                    ExtractFrame();

                    if (_queueFrame.Count == 0)
                        return false;

                    //isWaitingAck = false;

                    // dequeue received frame
                    var frameReceived = (byte[])_queueFrame.Dequeue();

                    // read acknowledge
                    byte[] acknowledge = { frameReceived[3], frameReceived[4] };

                    // return true or false if ACK or NACK
                    if (acknowledge[0] == PN532.ACK_PACKET_CODE[0] &&
                        acknowledge[1] == PN532.ACK_PACKET_CODE[1])
                        return true;
                }
            }
            else
            {
                throw new Exception("Could not write to NFC device");
            }

            return false;
        }

        public byte[] ReadNormalFrame()
        {
            _isWaitingAck = false;

            if (_received.WaitOne(WaitTimeout) && _queueFrame.Count > 0)
            {
                var frameRead = (byte[])_queueFrame.Dequeue();
                return frameRead;
            }

            return null;
        }

        public async Task WakeUp()
        {
            // PN532 Application Note C106 pag. 23

            // HSU wake up consist to send a SAM configuration command with a "long" preamble 
            // here we send preamble that will be followed by regular SAM configuration command
            byte[] pre1 = {0x55, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
            byte[] preamble = { 0x55, 0x55, 0x00, 0x00, 0x00 };
            await Write(pre1);
        }

        public void Close()
        {
            _rfidReader.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// HSU frame parser state
    /// </summary>
    internal enum HsuParserState
    {
        Preamble,
        StartCode,
        Length,
        LengthChecksum,
        FrameIdentifierAndData,
        DataChecksum,
        Postamble,
        AckCode
    }
}

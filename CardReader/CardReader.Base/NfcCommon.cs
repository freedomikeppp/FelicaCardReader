using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CardReader.Base;

namespace CardReader.Base
{
    class NfcCommon
    {
        public static IntPtr establishContext()
        {
            IntPtr context = IntPtr.Zero;

            uint ret = NfcApi.SCardEstablishContext(NfcConstant.SCARD_SCOPE_USER, IntPtr.Zero, IntPtr.Zero, out context);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                string message;
                switch (ret)
                {
                    case NfcConstant.SCARD_E_NO_SERVICE:
                        message = "サービスが起動されていません。";
                        break;
                    default:
                        message = "Smart Cardサービスに接続できません。code = " + ret;
                        break;
                }
                Console.WriteLine(message);
                return IntPtr.Zero;
            }
            Console.WriteLine("Smart Cardサービスに接続しました。");
            return context;
        }

        public static List<string> getReaders(IntPtr hContext)
        {
            uint pcchReaders = 0;

            uint ret = NfcApi.SCardListReaders(hContext, null, null, ref pcchReaders);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                return new List<string>();//リーダーの情報が取得できません。
            }

            byte[] mszReaders = new byte[pcchReaders * 2]; // 1文字2byte

            // Fill readers buffer with second call.
            ret = NfcApi.SCardListReaders(hContext, null, mszReaders, ref pcchReaders);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                return new List<string>();//リーダーの情報が取得できません。
            }

            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            string readerNameMultiString = unicodeEncoding.GetString(mszReaders);

            List<string> readersList = new List<string>();
            int nullindex = readerNameMultiString.IndexOf((char)0);   // 装置は１台のみ
            readersList.Add(readerNameMultiString.Substring(0, nullindex));
            return readersList;
        }

        public static NfcApi.SCARD_READERSTATE[] initializeReaderState(IntPtr hContext, List<string> readerNameList)
        {
            NfcApi.SCARD_READERSTATE[] readerStateArray = new NfcApi.SCARD_READERSTATE[readerNameList.Count];
            int i = 0;
            foreach (string readerName in readerNameList)
            {
                readerStateArray[i].dwCurrentState = NfcConstant.SCARD_STATE_UNAWARE;
                readerStateArray[i].szReader = readerName;
                i++;
            }
            uint ret = NfcApi.SCardGetStatusChange(hContext, 100/*msec*/, readerStateArray, readerStateArray.Length);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("リーダーの初期状態の取得に失敗。code = " + ret);
            }

            return readerStateArray;
        }

        public static void waitReaderStatusChange(IntPtr hContext, NfcApi.SCARD_READERSTATE[] readerStateArray, int timeoutMillis)
        {
            uint ret = NfcApi.SCardGetStatusChange(hContext, timeoutMillis/*msec*/, readerStateArray, readerStateArray.Length);
            switch (ret)
            {
                case NfcConstant.SCARD_S_SUCCESS:
                    break;
                case NfcConstant.SCARD_E_TIMEOUT:
                    throw new TimeoutException();
                default:
                    throw new ApplicationException("リーダーの状態変化の取得に失敗。code = " + ret);
            }

        }

        public static String readCard(IntPtr context, string readerName)
        {
            IntPtr hCard = connect(context, readerName);
            string cardId = readCardId(hCard);
            //Console.WriteLine("Reader name: " + readerName);
            //Console.WriteLine("CardID: " + cardId);
            disconnect(hCard);
            return cardId;
        }

        public static string readCardId(IntPtr hCard)
        {
            byte maxRecvDataLen = 64;
            byte[] recvBuffer = new byte[maxRecvDataLen + 2];
            byte[] sendBuffer = new byte[] { 0xff, 0xca, 0x00, 0x00, 0x00 };
            int recvLength = transmit(hCard, sendBuffer, recvBuffer);

            string cardId = BitConverter.ToString(recvBuffer, 0, recvLength - 2).Replace("-", "");
            return cardId;
        }
        
        public static IntPtr connect(IntPtr hContext, string readerName)
        {
            IntPtr hCard = IntPtr.Zero;
            IntPtr activeProtocol = IntPtr.Zero;
            uint ret = NfcApi.SCardConnect(hContext, readerName, NfcConstant.SCARD_SHARE_SHARED, NfcConstant.SCARD_PROTOCOL_T1, ref hCard, ref activeProtocol);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("カードに接続できません。code = " + ret);
            }
            return hCard;
        }

        public static void disconnect(IntPtr hCard)
        {
            uint ret = NfcApi.SCardDisconnect(hCard, NfcConstant.SCARD_LEAVE_CARD);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("カードとの接続を切断できません。code = " + ret);
            }
        }

        public static int transmit(IntPtr hCard, byte[] sendBuffer, byte[] recvBuffer)
        {
            NfcApi.SCARD_IO_REQUEST ioRecv = new NfcApi.SCARD_IO_REQUEST();
            ioRecv.cbPciLength = 255;

            int pcbRecvLength = recvBuffer.Length;
            int cbSendLength = sendBuffer.Length;
            IntPtr SCARD_PCI_T1 = getPciT1();
            uint ret = NfcApi.SCardTransmit(hCard, SCARD_PCI_T1, sendBuffer, cbSendLength, ioRecv, recvBuffer, ref pcbRecvLength);
            if (ret != NfcConstant.SCARD_S_SUCCESS)
            {
                throw new ApplicationException("カードへの送信に失敗しました。code = " + ret);
            }
            Console.WriteLine("");
            return pcbRecvLength; // 受信したバイト数(recvBufferに受け取ったバイト数)
        }

        private static IntPtr getPciT1()
        {
            IntPtr handle = NfcApi.LoadLibrary("Winscard.dll");
            IntPtr pci = NfcApi.GetProcAddress(handle, "g_rgSCardT1Pci");
            NfcApi.FreeLibrary(handle);
            return pci;
        }
    }
}

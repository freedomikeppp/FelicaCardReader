/*
カード読み取り定数

各PC/SC関数で使用されるパラメータの定数、及び戻り値の定数。

参考）PC/SC APIを用いてSuicaカードの利用履歴情報の読み取り
https://tomosoft.jp/design/?p=5543
*/
using System;

namespace CardReader.Base
{
    class NfcConstant
    {
        public const uint SCARD_S_SUCCESS = 0;
        public const uint SCARD_E_NO_SERVICE = 0x8010001D;
        public const uint SCARD_E_TIMEOUT = 0x8010000A;

        public const uint SCARD_SCOPE_USER = 0;
        public const uint SCARD_SCOPE_TERMINAL = 1;
        public const uint SCARD_SCOPE_SYSTEM = 2;

        public const int SCARD_STATE_UNAWARE = 0x0000;
        public const int SCARD_STATE_CHANGED = 0x00000002;// This implies that there is a
        // difference between the state believed by the application, and
        // the state known by the Service Manager.  When this bit is set,
        // the application may assume a significant state change has
        // occurred on this reader.
        public const int SCARD_STATE_PRESENT = 0x00000020;// This implies that there is a card
        // in the reader.
        public const UInt32 SCARD_STATE_EMPTY = 0x00000010;  // This implies that there is not
                                                             // card in the reader.  If this bit is set, all the following bits will be clear.

        public const int SCARD_SHARE_SHARED = 0x00000002; // - This application will allow others to share the reader
        public const int SCARD_SHARE_EXCLUSIVE = 0x00000001; // - This application will NOT allow others to share the reader
        public const int SCARD_SHARE_DIRECT = 0x00000003; // - Direct control of the reader, even without a card


        public const int SCARD_PROTOCOL_T0 = 1; // - Use the T=0 protocol (value = 0x00000001)
        public const int SCARD_PROTOCOL_T1 = 2;// - Use the T=1 protocol (value = 0x00000002)
        public const int SCARD_PROTOCOL_RAW = 4;// - Use with memory type cards (value = 0x00000004)

        public const int SCARD_LEAVE_CARD = 0; // Don't do anything special on close
        public const int SCARD_RESET_CARD = 1; // Reset the card on close
        public const int SCARD_UNPOWER_CARD = 2; // Power down the card on close
        public const int SCARD_EJECT_CARD = 3; // Eject the card on close
    }
}
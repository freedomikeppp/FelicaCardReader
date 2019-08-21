# FelicaCardReader

ACS ACR1252 のカードリーダを使って、FelicaカードのIDを取得するC#のサンプルプログラムです。

Windows環境(Windows10)で、標準で利用可能な「WinScard.dll」を使います。

自身の環境で実装する際のヒントとしてお使い下さい。

# 実装機能

- 接続されているカードリーダの取得、及びカードIDのリード。

# 参考情報

## 以下のサイトを参考にしました
[PC/SC APIを用いてSuicaカードの利用履歴情報の読み取り](https://tomosoft.jp/design/?p=5543)

## 使い方
```
# 接続の確立
IntPtr context = NfcCommon.establishContext();

# リーダの取得
List<string> readersList = NfcCommon.getReaders(context);

# リーダの状態初期化
NfcApi.SCARD_READERSTATE[] readerStateArray = NfcCommon.initializeReaderState(context, readersList);

while (true)
{
    # 指定した時間ごとにリーダの状態を取得
    NfcCommon.waitReaderStatusChange(context, readerStateArray, 1000);
    
    # カードリーダが存在しない場合
    if (readerStateArray != null && readerStateArray.Length == 0)
    {
        Console.WriteLine(new Exception("カードリーダが認識、または存在しませんでした。"));
    }
    # カードの状態取得
    else if ((readerStateArray[0].dwEventState & NfcConstant.SCARD_STATE_PRESENT) == NfcConstant.SCARD_STATE_PRESENT)
    {
        # カードID取得
        cardId = NfcCommon.readCard(context, readerStateArray[0].szReader);
    }
    Thread.Sleep(1000);
}

```

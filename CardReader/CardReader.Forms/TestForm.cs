using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using CardReader.Base;

namespace CardReader.Forms
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class TestForm : Form
    {
        TextBox textBox = new TextBox();

        public TestForm()
        {
            SuspendLayout();
            this.Size = new Size(500, 400);

            Label testLabel = new Label();
            testLabel.Text = "Card reader test";
            testLabel.ForeColor = Color.DarkGray;
            testLabel.Location = new Point(10, 10);
            testLabel.Size = new Size(150, 20);
            testLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(testLabel);

            textBox.Location = new Point(10, 35);
            textBox.Size = new Size(460, 300);
            textBox.Multiline = true;
            Controls.Add(textBox);
            
            //別タスクでリーダを1秒ごとに読み取り
            IntPtr context = NfcCommon.establishContext();
            List<string> readersList = NfcCommon.getReaders(context);
            NfcApi.SCARD_READERSTATE[] readerStateArray = NfcCommon.initializeReaderState(context, readersList);
            String cardId = "";

            Task task = new Task(() =>
            {
                while (true)
                {
                    NfcCommon.waitReaderStatusChange(context, readerStateArray, 1000);
                    if (readerStateArray != null && readerStateArray.Length == 0)
                    {
                        Console.WriteLine(new Exception("カードリーダが認識、または存在しませんでした。"));
                    }
                    else if ((readerStateArray[0].dwEventState & NfcConstant.SCARD_STATE_PRESENT) == NfcConstant.SCARD_STATE_PRESENT)
                    {
                        cardId = NfcCommon.readCard(context, readerStateArray[0].szReader);
                        _write_delegate(cardId);
                    }
                    Thread.Sleep(1000);
                }
            });
            task.Start();
        }


        /// <summary>
        /// 別スレッドのカードリーダタスクから呼び出す
        /// </summary>
        delegate void WriteDelegate(string cardId);
        private void _write_delegate(string cardId)
        {
            WriteDelegate delegateMethod = _write;
            this.Invoke(delegateMethod, new object[] { cardId });
        }

        private void _write(string cardId)
        {
            try
            {
                textBox.Text += "CardID: " + cardId + "\r\n";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;

namespace PokerClock
{
    public partial class Form1 : Form
    {
        //ストラクチャーデータのサイズとインデックス
        const int DATA_SIZE = 4;
        const int SB = 0, BB = 1, ANTE = 2, TIME = 3;
        //タイマーの処理間隔(Hz)
        const int TIMER_HZ = 10;
        //サウンドファイル
        const string SOUND_FILE= "sound.wav";

        //ストラクチャーデータ(一次元)
        List<int> blind_structure = new List<int>();

        //クロックの動作フラグ
        bool clock_enable = false;
        //開始時間
        DateTime startTime=DateTime.Now;
        //開始時の経過時間
        int initTime = 0;
        //経過時間
        int time = 0;
        //サウンドプレイヤー
        SoundPlayer soundPlayer;

        public Form1()
        {
            InitializeComponent();
        }

        //ファイル読み込み
        private void button3_Click(object sender, EventArgs e)
        {
            //ダイアログ表示
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //決定ボタンを押した
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //ファイルを開く
                    Stream stream = openFileDialog.OpenFile();

                    //読み込む
                    using(StreamReader reader=new StreamReader(stream))
                    {
                        try
                        {
                            //ストラクチャーデータを削除
                            blind_structure.Clear();
                            //1行読み込み
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                //直接数値に変換して保存
                                blind_structure.AddRange(line.Split(',').Select(s => int.Parse(s)).ToList());
                            }
                            //再描画
                            RepaintText();
                            //完了通知
                            label4.Text = "Load Complete";
                        }
                        catch(FormatException exception)
                        {
                            //例外発生時はストラクチャーデータはnullとする
                            blind_structure.Clear();
                            //エラー通知
                            label4.Text = "Error";
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //クロック動作を反転
            clock_enable = clock_enable ? false : true;
            //開始時間のリセット
            startTime = DateTime.Now;
            //開始時経過時間を設定
            initTime = time;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }
                //経過時間をリセット
                time = 0;
                //再描画
                RepaintText();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }
            //残り時間とブラインドレベルのインデックス
            int limit, index;
            (limit, index) = LimitTime();

            //リスト外部への参照防止
            if (index <= 0)
            {
                time = 0;
            }
            else
            {
                time -= limit + blind_structure[(index - 1) * DATA_SIZE + TIME]*60;
            }

            RepaintText();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }
            //残り時間とブラインドレベルのインデックス
            int limit, index;
            (limit, index) = LimitTime();

            //リスト外部への参照防止
            if ((index+1)*DATA_SIZE >= blind_structure.Count)
            {
                time = int.MaxValue;
            }
            else
            {
                time +=limit;
            }

            RepaintText();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }

                time -= 10 * 60;

            RepaintText();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }

                time -= 1 * 60;

            RepaintText();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }

                time -= 10;

            RepaintText();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }

                time += 10;

            RepaintText();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }

                time += 1*60;

            RepaintText();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //クロック停止時のみ動作
            if (clock_enable)
            {
                return;
            }

                time += 10*60;

            RepaintText();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //クロック動作時
            if (clock_enable)
            {
                if (DateTime.Now.AddSeconds((time-initTime+1)*(-1)) >= startTime)
                {
                    //時間を進める
                    time++;
                    //再描画
                    RepaintText();
                }
            }
        }

        private void RepaintText()
        {
            //残り時間とブラインドレベルのインデックス
            int limit, index;
            (limit, index) = LimitTime();

            //時間表示
            string nowTime = "0:00";
            if (index * DATA_SIZE < blind_structure.Count)
            {
                nowTime = ((limit / 60) + ":" + string.Format("{0:D2}", (limit % 60)));
            }
            //現在のブラインド表示
            string nowBlind = "End Tournament";
            if (index * DATA_SIZE < blind_structure.Count)
            {
                nowBlind =
                    blind_structure[index * DATA_SIZE + SB] + "/" + blind_structure[index * DATA_SIZE + BB] +
                    Environment.NewLine + blind_structure[index * DATA_SIZE + ANTE];
            }
            //次のブラインド表示
            string nextBlind = "Final Blind";
            if ((index + 1) * DATA_SIZE < blind_structure.Count)
            {
                nextBlind =
                    "Next" + Environment.NewLine +
                    blind_structure[(index + 1) * DATA_SIZE + SB] + "/" + blind_structure[(index + 1) * DATA_SIZE + BB] +
                    Environment.NewLine + blind_structure[(index + 1) * DATA_SIZE + ANTE];
            }

            //表示を更新する
            label1.Text = nowTime;
            label2.Text = nowBlind;
            label3.Text = nextBlind;

            //ブラインドアップ効果音
            if (limit <= 1)
            {
                soundPlayer = new SoundPlayer(SOUND_FILE);
                soundPlayer.Play();
            }
        }

        private (int,int) LimitTime()
        {
            //値をいじるのでコピー
            int limit = time;
            //注目しているレベルの時間
            int levelTime;
            //注目しているレベルのインデックス
            int index = 0;
            //現在のレベルに到達するまで探索
            while (limit >= (levelTime = blind_structure[index * DATA_SIZE + TIME] * 60))
            {
                //レベルでの所要時間を消費
                limit -= levelTime;
                //インデックスをずらす
                index++;
                //リスト外部の参照防止
                if(index * DATA_SIZE >= blind_structure.Count)
                {
                    return (0, index);
                }
            }

            limit = blind_structure[index * DATA_SIZE + TIME] * 60 - limit;

            return (limit,index);
        }
    }
}

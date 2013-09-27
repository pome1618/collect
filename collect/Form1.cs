using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace collect
{
	public partial class Form1 : Form
	{
		WebClient wc,dl;
		Stream st;
		StreamReader sr;
		string url,pic_url;

		public Form1()
		{
			InitializeComponent();
			wc = new WebClient();
			dl = new WebClient();
			textBox1.Text = "http://www.tomakomai-ct.ac.jp/";
			textBox2.Text = "500";
			textBox3.Text = "500";
			comboBox1.SelectedIndex = 0;
			comboBox2.SelectedIndex = 0;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			url = textBox1.Text;						//URLを取得
			st = wc.OpenRead(url);						//URLでStreamを作り
			sr = new StreamReader(st, Encoding.UTF8);	//StreamでStreamReaderを作る
			int file_num = 0;							//ファイルの名前
			string fe = comboBox1.Text;					//filename extension
			Image image;
			string path=@System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\collect\\";
			string line = "";
			int str_start = 0, str_end = 0, count = 0;

			if (!System.IO.Directory.Exists(@path))
			{
				//もしMyDocuments/collectがなかったら、つくる。
				System.IO.Directory.CreateDirectory(@path);
			}
			while (System.IO.File.Exists(@path + file_num+fe))
			{
				file_num++;
			}
			MessageBox.Show(Convert.ToString(file_num));

			while ((line = sr.ReadLine()) != null)	//もし読み込みに失敗したら終了。
			{
				str_end = line.IndexOf(fe);		//拡張子を書いてあるとこを探す。

				str_start = str_end;				//後ろから前に"を探していく
				while (str_start > -1)				//もし、そもそも拡張子が見つからなかったなら、このループは通らない。
				{
					if (line[str_start] == '"')
					{
						str_start++;				//ちょっとした調整
						str_end += 3;
						break;
					}
					count++;						//文字数のカウント
					str_start--;					//もう一つ前を見る
				}
				string msg = System.String.Format("str_start={0},str_end={1}", str_start, str_end);
			//	MessageBox.Show(msg);
				if(str_start>0&&str_end>0){			//もし画像があったら
					pic_url = line.Substring(str_start, count + 3);

					image = pic_url.IndexOf("http://") == -1 ? loadImageFromURL(url + pic_url) : image = loadImageFromURL(pic_url);
					if (check_data(comboBox2.SelectedIndex,Convert.ToInt16(textBox2.Text),Convert.ToInt16(textBox3.Text),image))
					{

						while (System.IO.File.Exists(@path + file_num + fe))
						{
							file_num++;
						}
						if (pic_url.IndexOf("http://") == -1)	//相対パス
						{
							dl.DownloadFile(url + pic_url, path + file_num + fe);
						}
						else									//絶対パス
						{
							dl.DownloadFile(pic_url, path + file_num + fe);
						}
						msg = System.String.Format("result={0}", path + file_num + fe);
						MessageBox.Show(msg);
					}
					else
					{
						file_num--;
					}
					file_num++;
				}
				count = 0;
			}
		}

		public static bool check_data(int cmd,int height,int width,Image image)
		{
			bool result=false;
			if (cmd == 0)
			{
				if (image.Height >= height && image.Width >= width)
				{
					result = true;
				}
				else
				{
					result = false;
				}
			}
			if (cmd == 1)
			{
				if (image.Height <= height && image.Width <= width)
				{
					result = true;
				}
				else
				{
					result = false;
				}
			}

			return result;
		}

		//---------------------------------------------------------------------------
		/// <summary>
		/// 指定されたURLの画像をImage型オブジェクトとして取得する
		/// </summary>
		/// <param name="url">画像データのURL(ex: http://example.com/foo.jpg) </param>
		/// <returns>         画像データ</returns>
		//---------------------------------------------------------------------------
		public static System.Drawing.Image loadImageFromURL(string url)
		{
			int buffSize = 65536; // 一度に読み込むサイズ
			MemoryStream imgStream = new MemoryStream();

			//------------------------
			// パラメータチェック
			//------------------------
			if (url == null || url.Trim().Length <= 0)
			{
				return null;
			}

			//----------------------------
			// Webサーバに要求を投げる
			//----------------------------
			WebRequest req = WebRequest.Create(url);
			BinaryReader reader = new BinaryReader(req.GetResponse().GetResponseStream());

			//--------------------------------------------------------
			// Webサーバからの応答データを取得し、imgStreamに保存する
			//--------------------------------------------------------
			while (true)
			{
				byte[] buff = new byte[buffSize];

				// 応答データの取得
				int readBytes = reader.Read(buff, 0, buffSize);
				if (readBytes <= 0)
				{
					// 最後まで取得した->ループを抜ける
					break;
				}

				// バッファに追加
				imgStream.Write(buff, 0, readBytes);
			}

			return new Bitmap(imgStream);
		}
	}
}
using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ComMaster
{
    public partial class Form1 : Form
    {
        private long receieve_count = 0;
        private long send_count = 0;
        private StringBuilder sb = new StringBuilder();
        private DateTime current_time = new DateTime();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //获取电脑当前可用串口并添加到选项列表
            comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            //批量添加波特率列表
            string[] baud = { "1200", "2400","4800", "9600", "14400", "19200", "38400", "115200" };
            comboBox2.Items.AddRange(baud);

            //设置默认值
            comboBox1.Text = "COM1";
            comboBox2.Text = "9600";
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "1";

            //初始时的状态栏显示
            label7.Text = "The serial port is closed";
            label7.ForeColor = Color.Red;
            label8.Text = "Tx:" + send_count.ToString() + " Bytes";
            label9.Text = "Rx:" + receieve_count.ToString() + " Bytes";
            label10.Text = "V1.0";
            label10.ForeColor = Color.Blue;
            button2.Enabled = false;
        }

        //打开串口按钮
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //将可能产生异常的处理代码放在try块中，根据当前串口属性来判断是否打开
                if(serialPort1.IsOpen)
                {
                    //串口已经打开
                    serialPort1.Close();
                    button2.Enabled = false;
                    //更新状态栏
                    label7.Text = "The serial port is closed";
                    label7.ForeColor = Color.Red;

                    button1.Text = "Open";
                    button1.BackColor = Color.ForestGreen;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                }
                else
                {
                    //如果点击按钮时串口是关闭的，说明当前操作是要打开串口
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    //根据设置进行串口配置              
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.DataBits = Convert.ToInt16(comboBox3.Text);

                    if (comboBox4.Text.Equals("None"))
                        serialPort1.Parity = System.IO.Ports.Parity.None;
                    else if(comboBox4.Text.Equals("Odd"))
                        serialPort1.Parity = System.IO.Ports.Parity.Odd;
                    else if (comboBox4.Text.Equals("Even"))
                        serialPort1.Parity = System.IO.Ports.Parity.Even;
                    else if (comboBox4.Text.Equals("Mark"))
                        serialPort1.Parity = System.IO.Ports.Parity.Mark;
                    else if (comboBox4.Text.Equals("Space"))
                        serialPort1.Parity = System.IO.Ports.Parity.Space;

                    if (comboBox5.Text.Equals("1"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBox5.Text.Equals("1.5"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBox5.Text.Equals("2"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.Two;

                    serialPort1.Open();//设置完毕后打开串口

                    //更新状态栏
                    label7.Text = "The serial port is open";
                    label7.ForeColor = Color.Green;
                    button1.Text = "Close";
                    button1.BackColor = Color.Firebrick;
                    button2.Enabled = true; //使能发送按钮
                }
            }
            catch(Exception ex)
            {
                //捕获可能发生的异常并进行处理

                //捕获到异常，创建一个新的对象，之前不能在用
                serialPort1 = new System.IO.Ports.SerialPort();

                //刷新COM选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

                //显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "Open";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);    //显示异常问题
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }
        }

        //发送按钮
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //串口处于开启状态，发送缓冲区内容
                if (serialPort1.IsOpen)
                {
                    int send_byte_num = 0;//本次发送字节数
                    sb.Clear(); //防止出错，先清空字符串构造器

                    //判断发送模式
                    if (radioButton3.Checked)
                    {
                        //HEX模式发送
                        //首先用正则表达式将用户输入的16进制字符匹配出来
                        string buf = textBox_T.Text;
                        string patten = @"\s";
                        string replacement = "";
                        Regex rgx = new Regex(patten);
                        string send_data = rgx.Replace(buf, replacement);

                        //判断字符串长度，不足2位 默认前置补零
                        if (send_data.Length % 2 > 0)
                        {
                            byte[] bytes = new byte[send_data.Length / 2 + 1];
                            for (var x = 0; x < bytes.Length - 1; x++)
                            {
                                var i = Convert.ToInt32(send_data.Substring(x * 2, 2), 16);
                                bytes[x] = (byte)i;
                            }

                            bytes[send_data.Length / 2] = Convert.ToByte(send_data.Substring(send_data.Length - 1, 1));
                            serialPort1.Write(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            byte[] bytes = new byte[send_data.Length / 2];
                            for (var x = 0; x < bytes.Length; x++)
                            {
                                var i = Convert.ToInt32(send_data.Substring(x * 2, 2), 16);
                                bytes[x] = (byte)i;
                            }
                            serialPort1.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else
                    {
                        //ASCII模式发送
                        //判断是否发送新行
                        if(checkBox4.Checked)
                        {
                            serialPort1.WriteLine(textBox_T.Text);
                            send_byte_num = textBox_T.Text.Length + 2;  //回车占2个字节
                        }
                        else
                        {
                            //不发送新行
                            serialPort1.Write(textBox_T.Text);
                            send_byte_num = textBox_T.Text.Length;
                        }
                    }

                    send_count += send_byte_num;    //计数变量刷新
                   
                    sb.Clear(); //防止出错，先清空字符串构造器

                    //显示到接收文本框内
                    try
                    {
                        Invoke((EventHandler)(delegate
                        {
                            label8.Text = "Tx:" + send_count.ToString() + "Bytes";  //刷新显示

                            if (checkBox1.Checked)
                            {
                                //显示时间
                                current_time = System.DateTime.Now;
                                sb.Append("[" + current_time.ToString("yyyy-MM-dd HH:mm:ss") + "]  ");
                            }

                            sb.Append("Send ");

                            if (radioButton4.Checked)
                            {
                                sb.Append("ASCII >" + Environment.NewLine);
                            }
                            else
                            {
                                sb.Append("HEX >" + Environment.NewLine);
                            }

                            sb.Append(textBox_T.Text);
                            if (checkBox2.Checked)
                            {
                                //接收自动换行
                                sb.Append(Environment.NewLine);
                            }

                            textBox_R. SelectionColor = Color.Blue;
                            textBox_R.AppendText(sb.ToString());
                        }));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                
            }
            catch(Exception ex)
            {
                serialPort1.Close();
                //捕获到异常，创建一个新的对象，之前不能在用
                serialPort1 = new System.IO.Ports.SerialPort();

                //刷新COM选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

                //显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "Open";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);    //显示异常问题
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }
        }

        //串口接受事件处理
        private void SerialPort1_DataReceieved(object sender, SerialDataReceivedEventArgs e)
        {
            int num = serialPort1.BytesToRead;  //获取缓冲区字节数
            byte[] received_buf = new byte[num];

            receieve_count += num;
            serialPort1.Read(received_buf, 0, num); //将缓冲区数据读取到received_buf
          

            sb.Clear(); //防止出错，先清空字符串构造器
            
            //显示到接收文本框内
            try
            {
                Invoke((EventHandler)(delegate
                {
                    //更新状态栏
                    label9.Text = "Rx:" + receieve_count.ToString() + "Bytes";
                    if (checkBox1.Checked)
                    {
                        //显示时间
                        current_time = System.DateTime.Now;
                        sb.Append("[" + current_time.ToString("yyyy-MM-dd HH:mm:ss") + "]  ");
                    }
                    
                    sb.Append("Recv ");

                    if (radioButton1.Checked)
                    {
                        sb.Append("ASCII >" + Environment.NewLine);
                    }
                    else
                    {
                        sb.Append("HEX >" + Environment.NewLine);
                    }

                    if (radioButton1.Checked)
                    {
                        sb.Append(Encoding.ASCII.GetString(received_buf));
                    }
                    else
                    {
                        sb.Append(BitConverter.ToString(received_buf).Replace("-"," "));
                    }

                    if (checkBox2.Checked)
                    {
                        //接收自动换行
                        sb.Append(Environment.NewLine);
                    }

                    textBox_R.SelectionColor = Color.Green;
                    textBox_R.AppendText(sb.ToString());
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        //清空接收按钮
        private void button3_Click(object sender, EventArgs e)
        {
            textBox_R.Text = "";
            receieve_count = 0;
            label9.Text = "Rx:" + receieve_count.ToString() + "Bytes";
        }

        //清空发送按钮
        private void button4_Click(object sender, EventArgs e)
        {
            textBox_T.Text = "";
            send_count = 0;
            label8.Text = "Tx:" + send_count.ToString() + "Bytes";
        }

        //自动定时发送
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox3.Checked)
            {
                //选择自动发送
                numericUpDown1.Enabled = false;
                timer1.Interval = (int)numericUpDown1.Value;    //定时器赋值，单位：毫秒
                timer1.Start();
                label7.Text = "Automatically sending";
            }
            else
            {
                //取消选中，停止自动发送
                numericUpDown1.Enabled = true;
                timer1.Stop();
                label7.Text = "The serial port is open";
            }
        }

        //定时时间到
        private void timer1_tick(object sender, EventArgs e)
        {
            button2_Click(button2, new EventArgs());//调用发送按钮的回调函数
        }
    }
}


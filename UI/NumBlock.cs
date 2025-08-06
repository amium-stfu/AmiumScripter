
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AmiumScripter.Helpers;

namespace AmiumScripter.UI
{

    public partial class NumBlock : Form
    {
        bool init = true;

        string range = "";
        object _min;
        object _max;
        bool isInt = false;


        // double variable
        private Func<double> getDoubleTarget = null;
        private Action<double> setDoubleTarget = null;

        void setIcons() 
        {

            btnPlusMinus.Text = "\u00B1";
            btnOk.Text = "\u2714";
            btnExit.Text = "\u274C";
        }
        public NumBlock(Func<double> doubleGetter, Action<double> doubleSetter, string text, string unit = "", double min = double.NaN, double max = double.NaN)
        {
            InitializeComponent();
            getDoubleTarget = doubleGetter;
            setDoubleTarget = doubleSetter;
            _min = min;
            _max = max;
            range = "";
            if (!double.IsNaN(min) && !double.IsNaN(max))
                range = $"{min} .. {max}";
            else if (!double.IsNaN(min) && double.IsNaN(max))
                range = $"{min} ..";
            else if (double.IsNaN(min) && !double.IsNaN(max))
                range = $".. {max}";
            else
                range = "";

            btnU1.Text = "";
            btnU2.Text = "";
            btnU3.Text = "";
            btnU4.Text = "";
            setIcons();
            Text = text + " " + range + " " + unit;
            tbResult.Text = getDoubleTarget().ToString().Replace(",",".");
            tbResult.Select();
        }


        // string variable
        private Func<string> getStringTarget = null;
        private Action<string> setStringTarget = null;
        public NumBlock(Func<string> stringGetter, Action<string> stringSetter, string text, string unit1 = "", string unit2 = "", string unit3 = "", string unit4 = "", double min = double.NaN, double max = double.NaN)
        {
            InitializeComponent();
            getStringTarget = stringGetter;
            setStringTarget = stringSetter;
            _min = min;
            _max = max;
            range = "";
            if (!double.IsNaN(min) && !double.IsNaN(max))
                range = $"{min} .. {max}";
            if (!double.IsNaN(min) && double.IsNaN(max))
                range = $"{min} ..";
            if (double.IsNaN(min) && !double.IsNaN(max))
                range = $".. {max}";

            btnU1.Text = unit1;
            btnU2.Text = unit2;
            btnU3.Text = unit3;
            btnU4.Text = unit4;


            setIcons();

            Text = text + " " + range;
            tbResult.Text = getStringTarget();
            tbResult.Select();
        }

        // int variable
        private Func<int> getIntTarget = null;
        private Action<int> setIntTarget = null;
        public NumBlock(Func<int> intGetter, Action<int> intSetter, string cText, string cUnit, int cMin = int.MinValue, int cMax = int.MaxValue)
        {
            InitializeComponent();
            _min = cMin;
            _max = cMax;
            getIntTarget = intGetter;
            setIntTarget = intSetter;
            if (cMin != int.MinValue && cMax != int.MaxValue)
                range = $"{cMin} .. {cMax}";
            else if (cMin != int.MinValue && cMax == int.MaxValue)
                range = $"{cMin} ..";
            else if (cMin == int.MinValue && cMax != int.MaxValue)
                range = $".. {cMax}";
            else
                range = "";

            btnU1.Text = "";
            btnU2.Text = "";
            btnU3.Text = "";
            btnU4.Text = "";

            setIcons();

            Text = cText + " " + range + " " + cUnit;
            tbResult.Text = getIntTarget().ToString();
            tbResult.Select();
            btnComma.Visible = false;

        }


        //Buttons
        private void btnClear_Click(object sender, EventArgs e)
        {
            tbResult.Text = "";
            tbResult.Select();
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;

            tbResult.Text += "1";
            btnOk.Select();
        }

        private void bnt2_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "2";
            btnOk.Select();
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "3";
            btnOk.Select();
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "4";
            btnOk.Select();
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "5";
            btnOk.Select();
        }

        private void btn6_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "6";
            btnOk.Select();
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "7";
            btnOk.Select();
        }

        private void btn8_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "8";
            btnOk.Select();
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "9";
            btnOk.Select();
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            if (init) tbResult.Text = "";
            init = false;
            tbResult.Text += "0";
            btnOk.Select();
        }

        private void btnComma_Click(object sender, EventArgs e)
        {
            if (tbResult.Text.Contains(".")) return;

            if (tbResult.Text == "")
                tbResult.Text += "0.";
            else
                tbResult.Text += ".";
            init = false;
            btnComma.Select();
        }

        private void btnPlusMinus_Click(object sender, EventArgs e)
        {
            if (tbResult.Text.ToString().Length == 0) return;

            var p = tbResult.Text.ToString().Substring(0, 1);





            if (p == "-")
            {
                tbResult.Text = tbResult.Text.Substring(1);
            }
            else
            {
                tbResult.Text = "-" + tbResult.Text;
            }
            init = false;
            this.Select();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
           
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (sendToTarget())
            {
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

        }

        private void tbResult_TextChanged(object sender, EventArgs e)
        {
            sendToTarget();
        }

        private bool sendToTarget()
        {
            if (setDoubleTarget != null)
                return sendDouble();

            if (setIntTarget != null)
                return sendInt();

            if (setStringTarget != null)
                return sendString();

            return false;


        }

        private bool sendString()
        {
            setStringTarget(tbResult.Text.ToString());
            return true;
        }

        private bool sendDouble()
        {
            double min = (double)_min;
            double max = (double)_max;

            Console.WriteLine(Name + "min + " + min + " .. max " + max);

            string input = tbResult.Text.Replace(",", ".");

            double number = input.ToDouble();
            if (!double.IsNaN(number))
            {
                if (number < min || number > max)
                {
                    MessageBox.Show("Out of range");
                    tbResult.Select();
                    return false;
                }

                if (setDoubleTarget != null)
                {
                    setDoubleTarget(number); // Use setter to update the double value
                }
                return true;
            }
            else
            {
                MessageBox.Show("Wrong format");
                tbResult.Select();
                return false;
            }

        }

        private bool sendInt()
        {

            int min = (int)_min;
            int max = (int)_max;

            string input = tbResult.Text.Replace(",", ".");
            if (int.TryParse(input, out int number))
            {
                if (number < min || number > max)
                {
                    MessageBox.Show("Out of range");
                    tbResult.Select();
                    return false;
                }

                if (setIntTarget != null)
                {
                    setIntTarget(number); // Update Signal object
                }
                return true;
            }
            else
            {
                MessageBox.Show("Wrong format");
                tbResult.Select();
                return false;
            }

        }

        private void tbResult_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == '\r')
            //    sendToTarget();


        }

        private void tbResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sendToTarget())
                {
                    this.Close();
                }
            }
            if (e.KeyCode == Keys.Escape)
            {

                Close();
            }
            if (isInt)
            {
                if (e.KeyCode == Keys.Oemcomma || e.KeyCode == Keys.Decimal)
                {
                    e.SuppressKeyPress = true; // Suppress the key press
                }
            }
        }

        private void NumBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                return;
                if (sendToTarget())
                {
                    this.Close();
                }
            }
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void btn5_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void btnU1_Click(object sender, EventArgs e)
        {
            string result = new string(tbResult.Text.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            tbResult.Text = result + btnU1.Text;
        }

        private void btnU2_Click(object sender, EventArgs e)
        {
            string result = new string(tbResult.Text.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            tbResult.Text = result + btnU2.Text;
        }

        private void btnU3_Click(object sender, EventArgs e)
        {
            string result = new string(tbResult.Text.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            tbResult.Text = result + btnU3.Text;
        }

        private void btnU4_Click(object sender, EventArgs e)
        {
            string result = new string(tbResult.Text.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            tbResult.Text = result + btnU4.Text;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }

}

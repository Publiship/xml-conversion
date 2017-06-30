using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string localdir = Properties.Resources.localdir;

        public Form1()
        {
            InitializeComponent();
        }

        #region form events
        /// <summary>
        /// process xml invoices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonProcess_Click(object sender, EventArgs e)
        {
            string sep = ","; //field seperator
            string typ = "SI"; //invoice type
            string nme = DateTime.Now.ToString("ddMMyyhhmmss"); //csv name
            string[] fle = { nme, ".csv", null }; //output file type, output file namne, output content (generated below)

            //get xml file(s) named in listbox
            if (this.listFiles.Items.Count > 0)
            {
                for (int ix = 0; ix < this.listFiles.Items.Count; ix++)
                {
                    string xmf = string.Format(@localdir + "/{0}", this.listFiles.Items[ix].ToString());

                    //using auto-generated xsd to retrieve data from xml
                    XmlSerializer xsr = new XmlSerializer(typeof(AccountsReceivable));

                    //need to filestream into xsr
                    FileStream fsm = new FileStream(xmf, FileMode.Open);
                    XmlReader rdr = XmlReader.Create(fsm);

                    //declare an object variable of the type to be deserialized.
                    AccountsReceivable acc;
                    //use the Deserialize method to restore the object's state.
                    acc = (AccountsReceivable)xsr.Deserialize(rdr);

                    //close file stream
                    fsm.Close();

                    //ensure class is populated
                    if (acc != null)
                    {
                        //get the data needed for sage import csv
                        //type = sales invoice SI?
                        //account reference = invoiceno?
                        //nominal a/c ref
                        //department code = office?
                        //date = invoice date
                        //reference
                        //details = remarks?
                        //net amount
                        //tax code
                        //tax amount
                        //exchange rate
                        //extra reference
                        //user name
                        //project refn
                        //cost code refn

                        StringBuilder csv = new StringBuilder();
                        csv.Append(typ).Append(sep);
                        csv.Append(acc.Invoices.InvoiceInfo.InvoiceNo.ToString()).Append(sep);
                        //csv.Append(acc.Invoices.InvoiceInfo.InvoiceNo.ToString()).Append(sep);
                        csv.Append(acc.Invoices.InvoiceInfo.Office.ToString()).Append(sep);
                        csv.Append(acc.Invoices.InvoiceInfo.Remarks.ToString()).Append(sep);
                        csv.Append(acc.Invoices.InvoiceInfo.InvoiceDate.ToString()).Append(sep);

                        //check not empty 
                        if (csv.Length > 0)
                        {
                            fle[2] = csv.ToString();

                            //generate csv file
                            if (write_to_file(fle))
                            {
                                
                            }
                            //end write to file
                        }
                        //end if not empty
                    }
                    //end if
                }
                //end listbox enumeration

                //open generated file
                open_file(fle);
            }
            //end if count > 0
        }
        //end process click
               
         
        /// <summary>
        /// check for files on FTP site
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGet_Click(object sender, EventArgs e)
        {
            //reset progress bar
            int prg = 0;
            this.progressBarFTP.Value = prg;

            //get from resx
            string[] ftpinfo = { Properties.Resources.hostIP.ToString(), Properties.Resources.username.ToString(), Properties.Resources.password.ToString() };
            string[] folders = { Properties.Resources.AR, Properties.Resources.AP };
            string localdir = Properties.Resources.localdir;

            if (!Directory.Exists(localdir))
            {
                Directory.CreateDirectory(localdir);
            }
            else
            {
                //make sure dir is empty

            }            
            //new instance of ftp handler
            misccs.ftp_handler ftp = new misccs.ftp_handler(ftpinfo[0], ftpinfo[1], ftpinfo[2]);

            //step through server folders
            for (int nx = 0; nx < folders.Length; nx++)
            {
                string pth = "scmprofit/" + folders[nx];
                //get files from server directory
                List<string> lst = ftp.directoryListSimple(pth);

                if (lst != null)
                {
                    //download to local directory if it's not been processed already
                    for (int ix = 0; ix < lst.Count; ix++)
                    {
                        string fle = lst[ix].ToString();
                        if (!string.IsNullOrEmpty(fle)) {

                            ftp.download(pth + "/" + fle, localdir + "/" + fle);
                            //add to list
                            this.listFiles.Items.Add(fle);

                        }

                        //update progress bar
                        prg = ((100 / folders.Length) / lst.Count) * ix;
                        this.progressBarFTP.Increment(prg);
                    }
                    //end downloads

                    //show download in list
                    //this.listFiles.Items.AddRange(lst.ToArray());
                }
                //end lst not null
            }
            //end step through folders

            this.progressBarFTP.Increment(100);
        }
        //end button get files

        /// <summary>
        /// close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //end close button
        #endregion

        #region functions and subs
        protected bool write_to_file(string[] _file)
        {
            bool res; //result

            try
            {
                string pth = string.Format(@localdir + "/{0}{1}", _file);

                // if file does not exist create and write to file
                if (!File.Exists(pth))
                {
                    // Create a file to write to
                    File.WriteAllText(pth, _file[2]);
                }
                else
                {
                    // append text
                    File.AppendAllText(pth, string.Format("{0}{1}", Environment.NewLine, _file[2]));
                }
                res = true;
            }
            catch
            {
                res = false;
            }

            return res;
        }
        //end write to file

        protected void open_file(string[] _file)
        {
            string pth = string.Format(@localdir + "/{0}{1}", _file);

            try
            {
                Process prc = new Process();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = @pth;
                prc.StartInfo = psi;
                prc.Start();
            }
            catch (Exception Ex)
            {
                //MessageBox.Show(Ex.Message);
            }
        }
        //end open file
        #endregion

        #region deprecated code
        /// <summary>
        /// alternative method for processing xml file
        /// </summary>
        protected void readxmlintodataset()
        {
            DataSet ds = new DataSet();

            // update xml file location
            //ds.ReadXml(Server.MapPath("XMLFile3.xml"));
            //ds.ReadXmlSchema("c:/discrepancy/SCM_AR_20170617883742004.XML");

            ds.ReadXml("c:/discrepancy/SCM_AR_20170617883742004.XML", XmlReadMode.InferSchema);

            //if this is nested xml, there will be multiple tables not just one
            DataTable dtstr = new DataTable();
            dtstr = ds.Tables[0];

            // Change csv file name or path if needed
            //string strFilePath = Server.MapPath("XML2CSV.csv");
            string strcsv = "c:/discrepancy/Test.csv";

            System.IO.StreamWriter sw = new System.IO.StreamWriter(strcsv, false);

            // write columns
            int ix = dtstr.Columns.Count;

            for (int i = 0; i < ix; i++)
            {
                sw.Write(dtstr.Columns[i]);

                if (i < ix - 1)
                {
                    sw.Write(",");
                }
            }

            sw.Write(sw.NewLine);

            // write all the rows
            foreach (DataRow drString in dtstr.Rows)
            {
                for (int i = 0; i < ix; i++)
                {
                    if (!Convert.IsDBNull(drString[i]))
                    {
                        sw.Write(drString[i].ToString());
                    }

                    if (i < ix - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }

            sw.Close();

        }
        //end readxmlintodaatset
        #endregion


    }
}

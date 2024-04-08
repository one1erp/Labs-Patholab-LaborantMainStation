using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Patholab_Common; //for logging
using Patholab_DAL_V1;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace LaborantMainStation
{
   
    public partial class MainWindow : Window
    {
        private static DataLayer dal;
    
        public List<GridRow> Rows { get; set; }
        

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                this.DataContext = this;
                //initialize the date time on screen
                DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
                    {
                        this.dateText.Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
                    }, this.Dispatcher);

                //connect to data layer via app.config
                dal = new DataLayer();
                string connectionStrings = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
                dal.MockConnect(connectionStrings);

                //get laborant stations
                string phraseName = "Laborant Position";
                PHRASE_HEADER laborantPositions = dal.GetPhraseByName(phraseName);
                if (laborantPositions == null)
                {
                    CustomMessagbox(phraseName);

                    this.Close();
                    return;
                }
                //show stations on screen
                int i = 0;
                Rows = new List<GridRow>();
                //create a row with the name of station AND  a text box
                foreach (KeyValuePair<string, string> laborantPositionKV in laborantPositions.PhraseEntriesDictonary)
                {

                    var row = new GridRow();
                    row.StationName = laborantPositionKV.Value;
                    row.StationCode = laborantPositionKV.Key;
                    row.RowNember = i;
                    row.SaveNeeded = false;
                    Rows.Add(row);
                    i++;
                }
                Refresh_Click(null, null);
            }
            catch (Exception ex)
            {
                CustomMessagbox("אירעה תקלה, נא לפנות לתמיכה \n" + ex.ToString());
                Logger.WriteLogFile(ex);
            }
        }

        private void CustomMessagbox(string errormessage)
        {
            MessageBox.Show(this,errormessage, "",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error,
                            System.Windows.MessageBoxResult.OK,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }

     
        void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {

           
            foreach (GridRow row in Rows)
            {
                
                    if (row.LaborantWorkerNumber == "" || !row.SaveNeeded )
                    {
                        continue;
                    }
                    if (row.SessionStart !="" )
                    {
                        CustomMessagbox("לא ניתן לשמור שינויים בעמדה " + row.StationName + " מכיוון שהעמדה בשימוש");
                        Refresh_Click(null,null);
                        continue;
                    }
                    OPERATOR_USER opu = GetOperator(row.LaborantWorkerNumber);
                    if (opu == null)
                    {
                        //what to do if not recognized?
                        CustomMessagbox("קוד המשתמש ב "+ row.StationName +" אינו מזוהה. לא ניתן לשמור");
                        continue;
                    }
                    else
                    {
                        U_LABORANT_WORK laborantWork = new U_LABORANT_WORK();
                        laborantWork.U_LABORANT_WORK_ID = (long)dal.GetNewId("SQ_U_LABORANT_WORK");
                        laborantWork.NAME = laborantWork.U_LABORANT_WORK_ID.ToString();
                        
                        laborantWork.VERSION = "1";
                        laborantWork.VERSION_STATUS = "A";

                        U_LABORANT_WORK_USER laborantWorkUser = new U_LABORANT_WORK_USER();
                        laborantWorkUser.U_LABORANT_WORK_ID = laborantWork.U_LABORANT_WORK_ID;
                        laborantWorkUser.U_LABORANT = opu.OPERATOR_ID;
                        laborantWorkUser.U_LABORANT_POSITION = row.StationCode;
                        laborantWorkUser.U_BARCODE_READER = row.StationCode;
                        laborantWorkUser.U_STARTED_ON = DateTime.Now;
                        laborantWorkUser.U_STATUS = "O";
                        laborantWorkUser.U_TOTAL_NUMBER_BARCODE = 0;
                        dal.Add(laborantWork);
                        dal.Add(laborantWorkUser);
                        dal.SaveChanges();


                        row.LaborantId = laborantWork.U_LABORANT_WORK_ID.ToString();
                        row.LaborantName = opu.U_HEBREW_NAME;
                        row.SessionStart = laborantWorkUser.U_STARTED_ON.ToString();

                        row.SaveNeeded = false;
                        Refresh_Click(null,null);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);
            }
            }
        void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            foreach (GridRow row in Rows)
            {
                //check  for the latest entry
                U_LABORANT_WORK_USER laborantWorkUser =
                    dal.FindBy<U_LABORANT_WORK_USER>
                        (lu => lu.U_STATUS == "O"
                               && lu.U_LABORANT_POSITION == row.StationCode
                               && lu.U_STARTED_ON != null)
                       .OrderByDescending(lu => lu.U_STARTED_ON).FirstOrDefault();
                if (laborantWorkUser != null)
                {
                    row.LaborantId = laborantWorkUser.U_LABORANT.ToString();
                    row.LaborantWorkId = laborantWorkUser.U_LABORANT_WORK_ID;
                    OPERATOR_USER opu = dal.FindBy<OPERATOR_USER>(ou => ou.OPERATOR_ID == laborantWorkUser.U_LABORANT.Value).FirstOrDefault();
                    row.LaborantName = (opu.U_HEBREW_NAME ?? "");
                    row.LaborantWorkerNumber =opu.U_PATHOLAB_WORKER_NBR ;
                    row.SessionStart = laborantWorkUser.U_STARTED_ON.ToString();
                    

                    row.SaveNeeded = false;
                }
                else 
                if (!row.SaveNeeded)
                {
                    // clear the row if no data found
                    row.Clean();
                }
            }
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);
            }
        }


        private OPERATOR_USER GetOperator(string text)
        {
            if (String.IsNullOrEmpty(text)) return null;
            return dal.FindBy<OPERATOR_USER>(o => o.U_PATHOLAB_WORKER_NBR == text).FirstOrDefault();

        }

        private void EndSessionClick(object sender, RoutedEventArgs e)
        {
            try
            {

           
            Button button = sender as Button;
            GridRow row = (GridRow)dg.SelectedItem;
            EndSession(row);
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);
            }
        }

        private void EndSession(GridRow row)
        {
            try
            {
 
            if ( row.LaborantWorkId == 0)
            {
               CustomMessagbox("לא נמצאה שעת התחלה, לא ניתן לסיים");
                return;
            }
            
            U_LABORANT_WORK_USER laborantWorkUser = dal.FindBy<U_LABORANT_WORK_USER>
                (lu => lu.U_LABORANT_WORK_ID==row.LaborantWorkId).SingleOrDefault();
            if (laborantWorkUser.U_STATUS != "O")
            {
                CustomMessagbox("הרשומה המוצגת לא מעודכנת, לא ניתן לסיים עבודה. נא לרענן את המסך ולנסות שוב");
                return;
            }
            laborantWorkUser.U_STOPED_ON = DateTime.Now;
            laborantWorkUser.U_STATUS = "C"; 

            dal.SaveChanges();
            Refresh_Click(null,null);
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Save_Click(null,null);
            Cancel_Click(null,null);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }
    }
}

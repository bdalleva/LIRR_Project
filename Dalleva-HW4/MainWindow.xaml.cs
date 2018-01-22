using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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
using TrainStation;

namespace Dalleva_HW4
{
   

    //***********************************************************************
    //The main window is where several variables are declared to be used 
    //By the rest of the program.
    //***********************************************************************
    public partial class MainWindow : Window
    {

        string connString = @"server=(localdb)\MSSQLLocalDB;" + "integrated security=SSPI;" + "database=TrainSchedule;" + "MultipleActiveResultSets=True";
        private BranchSchedule schedule = new BranchSchedule();
        private StationCollection collection = new StationCollection();
        private TrainCollection trains = new TrainCollection();
        private Train train = new Train();
        private string stationName = "";
        private int id = 0;
        private string location = "";
        private string fareZone = "";
        private double mileageToPenn = 0;
        private string picFilename = "";
        Station temp = new Station();
        public MainWindow()
        {
            InitializeComponent();

        }
        //*******************************
        //Open Branch Schedule from JSON
        //Opens a dialog, and then uses
        //the file name to deserialize.
        //After, adds data to the listbox
        //*******************************
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
      
           
            string filename;
            OpenFileDialog open = new OpenFileDialog();
            string directory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(directory);
            open.DefaultExt = ".json";
            open.Filter = "Json files (*.json)|*.json";
            open.Title = "Open Branch Schedule From JSON";
            if (open.ShowDialog() == true)
            {
                filename = open.FileName;
                listBoxBranches.Items.Clear();
            }
            else
            {
                return;
            }
            
            FileStream reader = new FileStream(filename, FileMode.Open, FileAccess.Read);
            DataContractJsonSerializer inputserializer = new DataContractJsonSerializer(typeof(BranchSchedule));
            schedule = (BranchSchedule)inputserializer.ReadObject(reader);
            reader.Close();
            textBoxBranchId.Text = schedule.BranchID.ToString();
         
            foreach (int id in schedule.TrainIDS)
            {
              listBoxBranches.Items.Add(id);
            }

        }

        //**********************************
        //Event Handler to exit the program
        //**********************************
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //**********************************************************************************************************************
        //Import Stations from JSON opens a dialog, then deserializes, and then adds data to the server, and finally reads it.
        //Deleting the table happens before anything else to prevent any duplicate data or crashing identical primary key fields.
        //**********************************************************************************************************************
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
          
            string filename;
            OpenFileDialog open = new OpenFileDialog();
            string directory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(directory);
            open.DefaultExt = ".json";
            open.Filter = "Json files (*.json)|*.json";
            open.Title = "Import Stations From JSON";
            if (open.ShowDialog() == true)
            {
                deleteFromTable();
                filename = open.FileName;
                stationCollectionDeserialization(filename);
                foreach (Station s in collection.Stations)
                {
                    stationName = s.Name;
                    id = s.ID;
                    location = s.Location;
                    fareZone = s.FareZone.ToString();
                    mileageToPenn = s.MileageToPenn;
                    picFilename = s.PicFileName;
                    addDataToServer(id, stationName, location, fareZone, mileageToPenn.ToString(), picFilename);
                }
                getDataFromSQLReader();
            }
            else
            {
                return;
            }

           
        }
        //******************
        //About Menu Option
        //******************
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TrainSchedule\nVersion3.0\nWritten by Brandon D'Alleva", "About Train Schedule");
        }
        //***********************************************************
        //This is a function used to load data from the SQL server.
        //***********************************************************
        private void getDataFromSQLReader()
        {

            SqlConnection sqlConn;
            sqlConn = new SqlConnection(connString);
            sqlConn.Open();
            string sql = "SELECT * FROM Stations";
            SqlCommand command = new SqlCommand(sql, sqlConn);
            SqlDataReader reader = command.ExecuteReader();
            listBoxStations.DisplayMemberPath = "Name";
            listBoxStations.ItemsSource = reader;
            reader.Read();
        }
        //*********************************************
        //Private function to add data to SQL server
        //When called. Uses all appropriate fields
        //in the database.
        //*********************************************
        private void addDataToServer(int id, string stationName, string location, string fareZone, string mileageToPenn, string picFilename)
        {

            string sql2 = string.Format("INSERT INTO Stations" +
                "(StationId, Name, Location, FareZone, MileageToPenn, PicFilename) Values " +
                "('{0}','{1}','{2}','{3}','{4}','{5}')", id, stationName, location, fareZone, mileageToPenn, picFilename);
            SqlConnection sqlConn2 = new SqlConnection(connString);
            sqlConn2.Open();
            SqlCommand command3 = new SqlCommand(sql2, sqlConn2);
            int rowsAffected = command3.ExecuteNonQuery();


        }


        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //********************************************************
        //Method to de-serializes a StationCollection JSON file.
        //Used with the import from JSON menu option.
        //********************************************************
        private void stationCollectionDeserialization(string f)
        {

            FileStream reader = new FileStream(f, FileMode.Open, FileAccess.Read);
            DataContractJsonSerializer inputserializer = new DataContractJsonSerializer(typeof(StationCollection));
            collection = (StationCollection)inputserializer.ReadObject(reader);
            reader.Close();
        
        }

        //********************************************************
        //Method to de-serializes a TrainCollection JSON file.
        //Used on the loading of the window, to be used for
        //The branch tab.
        //********************************************************
        private void trainCollectionDeserialization()
        {
            FileStream reader = new FileStream("TrainCollection2.json", FileMode.Open, FileAccess.Read);
            DataContractJsonSerializer inputserializer = new DataContractJsonSerializer(typeof(TrainCollection));
            trains = (TrainCollection)inputserializer.ReadObject(reader);
            reader.Close();
        }

        //******************************************************
        //This method will delete data from the database 
        //before repopulating it with new data from a JSON file.
        //******************************************************
        private void deleteFromTable()
        {
            string sql3 = "TRUNCATE TABLE Stations";
            SqlConnection sqlconn3 = new SqlConnection(connString);
            sqlconn3.Open();
            SqlCommand command = new SqlCommand(sql3, sqlconn3);
            int rowAffected = command.ExecuteNonQuery();
        }
        //*******************************************************************
        //When the Window Opens, Populate the ListBox with the stations data
        //From the SQL Database.
        //*******************************************************************
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            getDataFromSQLReader();
            trainCollectionDeserialization();
        }
        //*******************************************************************
        //The list box. This is going to take data from the SQL server
        //And load the data into that list box.
        //Need to load the picture as well
        //*******************************************************************
        private void listBoxStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         

            int select = listBoxStations.SelectedIndex + 1;

            //******************************************************
            //This if statement was necessary, because
            //When I had an item selected in the listbox
            //And loaded the JSON File, the program
            //Would load the selection changed (this method) again,
            //And then Set selected to be 0.
            //******************************************************

            if (select == 0)
            {
                select = 1;
            }
            string sql3 = "SELECT * FROM Stations Where StationId ='" + select.ToString() + "'";


            SqlConnection sqlconn3 = new SqlConnection(connString);
            sqlconn3.Open();
            SqlCommand command = new SqlCommand(sql3, sqlconn3);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();

            textBoxName.Text = reader["Name"].ToString();
            textBoxLocation.Text = reader["Location"].ToString();
            textBoxId.Text = reader["StationId"].ToString();
            textBoxFareZone.Text = reader["FareZone"].ToString();
            textBoxMileageToPenn.Text = reader["MileageToPenn"].ToString();
            BitmapImage image = new BitmapImage(new Uri("/Images/"+reader["PicFilename"], UriKind.Relative));
            imageStationPictures.Source = image;

        


        }
        //********************************************************
        //ListBox for the branches section of the program.
        //Used to click on the train variable and show the ID and 
        //Time.
        //********************************************************
        private void listBoxBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!listViewStationArrivals.Items.IsEmpty)
            {
                listViewStationArrivals.Items.Clear();
            }

         

           
            int id = Convert.ToInt32(listBoxBranches.SelectedItem);

          
            train = trains.FindTrain(id);


            int i = 0;
            foreach (StationArrival s in train.StationArrivals)
            {

                listViewStationArrivals.Items.Add(train.StationArrivals[i]);
                i++;

            }
        }

        private void imageStationPictures_Loaded(object sender, RoutedEventArgs e)
        {
      
        }
    }
}

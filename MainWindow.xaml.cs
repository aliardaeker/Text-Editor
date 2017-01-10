using System;
using System.Collections.Generic;
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
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace TextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Data> rows = new ObservableCollection<Data>();
        ObservableCollection<Data> hiddenRows = new ObservableCollection<Data>();
        ObservableCollection<Data> copyRows = new ObservableCollection<Data>();
        Boolean fileFlag = false;
        string fileName = "";
        int fileCounter = 0;
        int focusPosition;
        bool firstOpen = true;
        bool ffOpen = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var command = textBox.Text;
                string[] commands = command.Split(' ');

                if (commands[0].ToLower() == "o" || commands[0].ToLower() == "open")
                {
                    fileCounter++;
                    rows.Clear();
                    #pragma warning disable CS0219 // Variable is assigned but its value is never used
                    focusPosition = 1;
                    #pragma warning restore CS0219 // Variable is assigned but its value is never used
                    int counter = 0;
                    fileFlag = true;
                    fileName = commands[1];
                    StreamReader file = new StreamReader(commands[1]);
                    string currentLine;

                    label1.Content = "File Name: " + fileName;
                    label2.Content = "Current Line Number: " + 1;

                    while ((currentLine = file.ReadLine()) != null)
                    {
                        counter++;
                        rows.Add(new Data { lineNum = counter, data = currentLine, suffix = "=====" });
                    }
                    dataGrid.ItemsSource = rows;

                    label3.Content = "Size: " + counter;
                    lineCursor.Content = "Line Number: ";
                    columnCursor.Content = "Column Number: ";
                    filesRing.Content = "Files: " + fileCounter;
                    file.Close();

                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(ProcessRows));
                }
                else if (commands[0].ToLower() == "save" && commands.Count() > 1 && commands[1].ToLower() == "as")
                {
                    StreamWriter file = new StreamWriter(commands[2]);

                    for (int i = 0; i < rows.Count(); i++) file.WriteLine(rows[i].data);

                    file.Close();
                    //cleanLabels();
                    MessageBox.Show("File saved as " + commands[2]);
                }
                else if (commands[0].ToLower() == "save")
                {
                    if (fileFlag)
                    {
                        StreamWriter file = new StreamWriter(fileName);

                        for (int i = 0; i < rows.Count(); i++) file.WriteLine(rows[i].data);

                        file.Close();
                        //cleanLabels();
                        MessageBox.Show("File overwriten");
                    }
                    else
                    {
                        if (commands.Count() == 1) MessageBox.Show("No file exists.\nTo create a file enter a path and a name after save command like this:\nSave C:\\Users\\arda\\Documents NewFile.txt");
                        else
                        {
                            string path = commands[1];
                            string name = commands[2];

                            StreamWriter file = new StreamWriter(path + "\\" + name);
                            file.Close();
                            //cleanLabels();
                            MessageBox.Show(name + " created at " + path);
                        }
                    }
                }
                else if (commands[0].ToLower() == "help" || commands[0].ToLower() == "h")
                {
                    MessageBox.Show("Save: Overwrites the original file.\nSave As: Saves the file to the name specified at command line.\n" +
                       "Open: Opens a specified file for editing.\nSearch: Allows the user to search for a string.\n" +
                       "#: Skip the line number of the file.\nUp #: Scroll up # lines.\nDown #: Scroll down # lines.\n" +
                       "Left #: Scroll left # lines.\nRight #: Scroll right # lines.\nForward: Scrolls one screenfull towards the end of file.\n" +
                       "Back: Scrolls one screenfull towards the top of file.\nSetcl #: Defines which line number is the current line.\n" +
                       "Change: Finds & modifies a searched string, starting with the defined current line.\nHelp: Usage of the commands.\n" +
                       "You can use * in change and replace commands to scan all the document.\nIn replace it scans from the current line if you use " +
                       "it in second command, or it scans all the file if you use it as third command. (Extra Credit)");
                }
                else if (commands[0].ToLower() == "search" || commands[0].ToLower() == "s" || commands[0].ToLower() == "find")
                {
                    if (fileFlag)
                    {
                        if (commands.Count() == 3)
                        {
                            string strSearching = commands[1].Split('/')[1];
                            int lineNum, i, numberOfFounds = 0, foundPosition = 0;

                            if (commands[1].Split('/')[2] == "*") lineNum = 0;
                            else lineNum = Int32.Parse(commands[1].Split('/')[2]) - 1;

                            int columnNum = Int32.Parse(commands[2]);
                            Boolean foundFlag = false, firstSearch = true;

                            for (i = lineNum; i < rows.Count(); i++)
                            {
                                if (firstSearch && rows[i].data.Substring(columnNum).Contains(strSearching))
                                {
                                    foundPosition = i;
                                    numberOfFounds++;
                                    foundFlag = true;
                                }
                                else if (!firstSearch && rows[i].data.Contains(strSearching))
                                {
                                    foundPosition = i;
                                    numberOfFounds++;
                                    foundFlag = true;
                                }
                                firstSearch = false;
                            }

                            if (foundFlag)
                            {
                                MessageBox.Show(strSearching + " is found " + numberOfFounds + " times.");
                                scrollViewer.ScrollToBottom();
                                focusPosition = foundPosition;
                                dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                                dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                            }
                            else MessageBox.Show(strSearching + " is not found.");
                        }
                        else MessageBox.Show("You should search like this:\nfind /mystr/#lines_from_current_line starting_column(if>1)");
                    }
                    else MessageBox.Show("No file opened.");
                }
                else if (commands[0].Contains("1") || commands[0].Contains("2") || commands[0].Contains("3") || commands[0].Contains("4") || commands[0].Contains("5")
                        || commands[0].Contains("6") || commands[0].Contains("7") || commands[0].Contains("8") || commands[0].Contains("9"))
                {
                    scrollViewer.ScrollToBottom();
                    int lNum = Int32.Parse(commands[0]);

                    for (int i = 0; i < rows.Count(); i++)
                    {
                        if (rows[i].lineNum == lNum)
                        {
                            focusPosition = i;
                            dataGrid.SelectedItem = dataGrid.Items[i];
                            dataGrid.ScrollIntoView(dataGrid.Items[i]);
                            break;
                        }
                    }
                }
                else if (commands[0].ToLower() == "u" || commands[0].ToLower() == "up")
                {
                    scrollViewer.ScrollToBottom();
                    int lineNum = focusPosition - Int32.Parse(commands[1]);

                    if (lineNum < 1)
                    {
                        focusPosition = 0;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                    else
                    {
                        focusPosition = lineNum;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                }
                else if (commands[0].ToLower() == "d" || commands[0].ToLower() == "down")
                {
                    scrollViewer.ScrollToBottom();
                    int lineNum = focusPosition + Int32.Parse(commands[1]);

                    if (lineNum > rows.Count() - 1)
                    {
                        focusPosition = rows.Count() - 1;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                    else
                    {
                        focusPosition = lineNum;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                }
                else if (commands[0].ToLower() == "l" || commands[0].ToLower() == "left")
                {
                    int range = Int32.Parse(commands[1]);
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - (range * 6));
                }
                else if (commands[0].ToLower() == "r" || commands[0].ToLower() == "right")
                {
                    int range = Int32.Parse(commands[1]);
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (range * 6));
                }
                else if (commands[0].ToLower() == "f" || commands[0].ToLower() == "forward")
                {
                    scrollViewer.ScrollToBottom();
                    double winHeight = mainWindow.Height;
                    double x;

                    if (firstOpen) x = winHeight * 0.028;
                    else x = winHeight * 0.034;

                    int lineNum = focusPosition - Convert.ToInt32(Math.Floor(x));

                    if (lineNum < 1)
                    {
                        focusPosition = 0;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                    else
                    {
                        focusPosition = lineNum;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                }
                else if (commands[0].ToLower() == "b" || commands[0].ToLower() == "backward")
                {
                    scrollViewer.ScrollToBottom();
                    double winHeight = mainWindow.Height;
                    double x;

                    if (firstOpen) x = winHeight * 0.028;
                    else x = winHeight * 0.034;

                    int lineNum = focusPosition + Convert.ToInt32(Math.Floor(x));

                    if (lineNum > rows.Count() - 1)
                    {
                        focusPosition = rows.Count() - 1;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                    else
                    {
                        focusPosition = lineNum;
                        dataGrid.SelectedItem = dataGrid.Items[focusPosition];
                        dataGrid.ScrollIntoView(dataGrid.Items[focusPosition]);
                    }
                }
                else if (commands[0].ToLower() == "setcl")
                {
                    int lineNum = Int32.Parse(commands[1]);
                    focusPosition = lineNum + focusPosition;

                    if (focusPosition > rows.Count() - 1)
                    {
                        focusPosition = rows.Count();
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(ProcessRows));
                        label2.Content = "Current Line Number = " + focusPosition;
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(ProcessRows));
                        label2.Content = "Current Line Number = " + focusPosition;
                    }
                }
                else if (commands[0].ToLower() == "c" || commands[0].ToLower() == "change" || commands[0].ToLower() == "replace")
                {
                    if (fileFlag)
                    {
                        if (commands.Count() == 2 || commands.Count() == 3)
                        {
                            string strSearch = commands[1].Split('/')[1];
                            string strReplace = commands[1].Split('/')[2];
                            int numOfLines, foundCounter = 0;
                            string correctedStr;
                            Boolean foundFlag = false;

                            if (commands[1].Split('/')[3] == "*") numOfLines = rows.Count();
                            else numOfLines = Int32.Parse(commands[1].Split('/')[3]);

                            if (commands[2] != null && commands[2] == "*")
                            {
                                for (int i = 0; i < rows.Count(); i++)
                                {
                                    if (rows[i].data.Contains(strSearch))
                                    {
                                        foundFlag = true;
                                        foundCounter++;

                                        correctedStr = rows[i].data.Replace(strSearch, strReplace);
                                        rows[i].data = correctedStr;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = focusPosition - 1; i < numOfLines && i < rows.Count(); i++)
                                {
                                    if (rows[i].data.Contains(strSearch))
                                    {
                                        foundFlag = true;
                                        foundCounter++;

                                        correctedStr = rows[i].data.Replace(strSearch, strReplace);
                                        rows[i].data = correctedStr;
                                    }
                                }
                            }                       

                            if (foundFlag)
                            {
                                dataGrid.ItemsSource = rows;
                                dataGrid.Items.Refresh();
                                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(ProcessRows));
                                MessageBox.Show(strSearch + " is replaced with " + strReplace + " " + foundCounter + " times.");
                            }
                            else MessageBox.Show(strSearch + " is not found.");
                        }
                        else MessageBox.Show("You should replace like this:\nchange /mystr/newstr/on_n_lines * (optinal)");
                    }
                    else MessageBox.Show("No file opened.");
                }
                else MessageBox.Show("Wrong input");

                textBox.Text = "";
            }
        }

        private void ProcessRows()
        {
            foreach (Data item in dataGrid.ItemsSource)
            {
                var row = dataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (item.lineNum == focusPosition) row.Foreground = Brushes.Red;
                else row.Foreground = Brushes.Black;
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            var command = textBox.Text;
            string[] commands = command.Split(' ');

            if (fileFlag)
            {
                StreamWriter file = new StreamWriter(fileName);

                for (int i = 0; i < rows.Count(); i++) file.WriteLine(rows[i].data);

                file.Close();
                cleanLabels();
                MessageBox.Show("File overwriten");
            }
            else
            {
                if (commands.Count() == 1) MessageBox.Show("No file exists.\nTo create a file enter a path and a name at command line like this:\nC:\\Users\\arda\\Documents NewFile.txt");
                else
                {
                    string path = commands[0];
                    string name = commands[1];

                    StreamWriter file = new StreamWriter(path + "\\" + name);
                    file.Close();
                    cleanLabels();
                    MessageBox.Show(name + " created at " + path);
                }
            }

            fileFlag = false;
        }

        private void saveAs_Click(object sender, RoutedEventArgs e)
        {
            var command = textBox.Text;
            string[] commands = command.Split(' ');

            if (commands[0] != "")
            {
                StreamWriter file = new StreamWriter(commands[0]);

                for (int i = 0; i < rows.Count(); i++) file.WriteLine(rows[i].data);

                file.Close();
                cleanLabels();
                MessageBox.Show("File saved as " + commands[0]);
            }
            else MessageBox.Show("Click 'Save As' after entering file name at the command line");
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save: Overwrites the original file.\nSave As: Saves the file to the name specified at command line.\n" +
                                   "Open: Opens a specified file for editing.\nSearch: Allows the user to search for a string.\n" +
                                   "#: Skip the line number of the file.\nUp #: Scroll up # lines.\nDown #: Scroll down # lines.\n" +
                                   "Left #: Scroll left # lines.\nRight #: Scroll right # lines.\nForward: Scrolls one screenfull towards the end of file.\n" +
                                   "Back: Scrolls one screenfull towards the top of file.\nSetcl #: Defines which line number is the current line.\n" +
                                   "Change: Finds & modifies a searched string, starting with the defined current line.\nHelp: Usage of the commands.\n" +
                                   "You can use * in change and replace commands to scan all the document.\nIn replace it scans from the current line if you use " +
                                   "it in second command, or it scans all the file if you use it as third command. (Extra Credit)");
        }

        private void dataGrid_KeyUp(object sender, KeyEventArgs e)
        {
            int counter = 0;
            Boolean itemFound = false;
            DataGridCell d = e.OriginalSource as DataGridCell;

            if (e.Key == Key.Enter)
            {
                if (d.Foreground.ToString() == "#FFFFFFFF")
                {
                    foreach (Data item in dataGrid.ItemsSource)
                    {
                        if (item.suffix[0] == 'i')
                        {
                            int numOfLines = (int) Char.GetNumericValue(item.suffix[1]);
                            for (int i = counter + 1; i < (counter + numOfLines + 1); i++) rows.Insert(i, new Data { lineNum = i, data = "", suffix = "=====" });

                            itemFound = true;
                            break;
                        }
                        else if (item.suffix[0] == 'x')
                        {
                            if (item.suffix == "x")
                            {
                                hiddenRows.Add(rows[counter + 1]);
                                rows.RemoveAt(counter + 1);
                                rows.Insert(counter + 1, new Data { lineNum = counter + 1, data = "hidden line", suffix = "=====" });
                            }
                            else
                            {
                                int numOfLines = (int)Char.GetNumericValue(item.suffix[1]);

                                for (int i = counter + 1; i < (counter + numOfLines + 1); i++) hiddenRows.Add(rows[i]);
                                for (int j = counter; j < (counter + numOfLines); j++) rows.RemoveAt(counter + 1);
                                rows.Insert(counter + 1, new Data { lineNum = counter + 1, data = "hidden line", suffix = "=====" });
                            }

                            itemFound = true;
                            break;
                        }
                        else if (item.suffix[0] == 's' && item.data == "hidden line")
                        {
                            if (item.suffix == "s")
                            {
                                rows.Insert(counter + 1, hiddenRows[(hiddenRows.Count() - 1)]);
                                hiddenRows.RemoveAt(hiddenRows.Count() - 1);
                                if (hiddenRows.Count() == 0) rows.RemoveAt(counter);
                            }
                            else
                            {
                                int numOfLines = (int) Char.GetNumericValue(item.suffix[1]);
                                int counterH = 0;

                                for (int i = counter + 1; i < (counter + numOfLines + 1); i++)
                                {
                                    rows.Insert(i, hiddenRows[(hiddenRows.Count() - numOfLines + counterH)]);
                                    counterH++;
                                }                               
  
                                for (int i = counter + 1; i < (counter + numOfLines + 1); i++) hiddenRows.RemoveAt(hiddenRows.Count() - 1);
                                if (hiddenRows.Count() == 0) rows.RemoveAt(counter);      
                            }

                            itemFound = true;
                            break;
                        }
                        else if (item.suffix[0] == 'a')
                        {
                            if (copyRows.Count() != 0)
                            {
                                int numOfLines = copyRows.Count();
                                int counterH = 0;

                                for (int i = counter + 1; i < (counter + numOfLines + 1); i++)
                                {
                                    string myData = copyRows[(copyRows.Count() - numOfLines + counterH)].data;
                                    rows.Insert(i, new Data { lineNum = counter + 1, data = myData, suffix = "=====" });
                                    counterH++;
                                }
                                copyRows.Clear();
                            }
                            else MessageBox.Show("You should copy or move some line first.");

                            itemFound = true;
                            break;
                        }
                        else if (item.suffix[0] == 'b')
                        {
                            if (copyRows.Count() != 0)
                            {
                                int numOfLines = copyRows.Count();
                                int counterH = 0;

                                for (int i = counter + 1; i < (counter + numOfLines + 1); i++)
                                {
                                    string myData = copyRows[(copyRows.Count() - numOfLines + counterH)].data;
                                    rows.Insert(i - 1, new Data { lineNum = counter + 1, data = myData, suffix = "=====" });
                                    counterH++;
                                }
                                copyRows.Clear();
                            }
                            else MessageBox.Show("You should copy or move some line first.");

                            itemFound = true;
                            break;
                        }
                        else if (item.suffix[0] == 'c')
                        {
                            if (item.suffix == "c") copyRows.Add(rows[counter]);
                            else
                            {
                                int numOfLines = (int)Char.GetNumericValue(item.suffix[1]);
                                for (int i = counter; i < (counter + numOfLines); i++) copyRows.Add(rows[i]);
                            }

                            itemFound = true;
                            break;
                        }
                        else if (item.suffix[0] == 'm')
                        {
                            if (item.suffix == "m")
                            {
                                copyRows.Add(rows[counter]);
                                rows.RemoveAt(counter);
                            }
                            else
                            {
                                int numOfLines = (int)Char.GetNumericValue(item.suffix[1]);
                                for (int i = counter; i < (counter + numOfLines); i++) copyRows.Add(rows[i]);
                                for (int j = counter; j < (counter + numOfLines); j++) rows.RemoveAt(counter);
                            }

                            itemFound = true;
                            break;
                        }
                        else if (item.suffix[0] == '“' || item.suffix[0] == '"')
                        {
                            string myData = rows[counter].data;
                            rows.Insert(counter + 1, new Data { lineNum = counter + 1, data = myData, suffix = "=====" });

                            itemFound = true;
                            break;
                        }

                        counter++;
                    }                   
                }
            }

            if (itemFound)
            {
                UpdateColumns();
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(ProcessRows));
            }
        }

        private void UpdateColumns()
        {
            int counter = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].suffix = "=====";
                rows[i].lineNum = i + 1;
                counter++;
            }

            label3.Content = "Size = " + counter;
            dataGrid.ItemsSource = rows;
            dataGrid.Items.Refresh();
        }

        private void cleanLabels()
        {
            label1.Content = "File Name:";
            label2.Content = "Current Line Number:";
            label3.Content = "Size: ";
            lineCursor.Content = "Line Number: ";
            columnCursor.Content = "Column Number: ";
            dataGrid.ItemsSource = null;
            dataGrid.Items.Refresh();
            fileFlag = false;
        }

        private void RowMouseEnter(object sender, MouseEventArgs e)
        {
            int index = dataGrid.ItemContainerGenerator.IndexFromContainer((DataGridRow)sender);
            lineCursor.Content = "Line Number: " + (index + 1);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (fileFlag)
            {
                int mousePosition = Convert.ToInt32(Math.Floor(e.GetPosition(dataGrid).X));
                int columnNo = mousePosition / 6;
                columnCursor.Content = "Column Number: " + columnNo.ToString();
            }
        }

        public void update_size(object sender, RoutedEventArgs e)
        {
            if (ffOpen) firstOpen = false;
            else ffOpen = true;
        }
    }

    public class Data
    {
        public int lineNum { get; set; }
        public string data { get; set; }
        public string suffix { get; set; }
    }
}

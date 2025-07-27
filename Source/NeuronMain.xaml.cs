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
using System.Windows.Shapes;

namespace Neuron
{
    public partial class NeuronMain : Window
    {
        public NeuronMain()
        {
            InitializeComponent();
            Commands.LoadContacts();

            string ChooseContact = null;

            while (true)
            {
                if (ChooseContact != null)
                {
                    Commands.UpdateMessages();
                }
            }
        }
    }
    internal class Commands()
    {
        public static void LoadMessages()
        {

        }
        public static void UpdateMessages()
        {

        }
        public static void SendMessage()
        {

        }
        public static void LoadContacts()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SC2_3DS.Helper;
using static SC2_3DS.Headers;
using static SC2_3DS.Weight;
using static SC2_3DS.Matrix;
using static SC2_3DS.Textures;
using static SC2_3DS.Objects;
using static SC2_3DS.ImportSC2;
using System.Collections;
using System.Runtime.InteropServices.JavaScript;

namespace SC2_3DS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReadData(object sender, RoutedEventArgs e)
        {
            var vmx = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "",
                DefaultExt = ".vmx",
                Filter = "SC2 Model | *.vmx; *.vmg",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            Endianness endianness = Endianness.LittleEndian;

            if (vmx.ShowDialog() == true)
            {
                byte[] vmxbytes = File.ReadAllBytes(vmx.FileName);
                using (MemoryStream input = new MemoryStream(vmxbytes))
                {
                    using (BinaryReader reader = new BinaryReader(input))
                    {
                        Byte[] fileCheck = reader.ReadBytes(4);
                        string magic = System.Text.Encoding.ASCII.GetString(fileCheck);
                        if (magic == "VMG.")
                        {
                            endianness = Endianness.BigEndian;
                        }
                        if (endianness == Endianness.LittleEndian) //Xbox
                        {
                            VMXObject vmxobject = ReadVMXObject(reader, input);
                            ExportTextures.TextureExportXbox(vmxobject);
                            ExportGLTF.Export(vmxobject);
                        }
                        else if (endianness == Endianness.BigEndian) //Gamecube
                        {
                            VMGObject vmgobject = ReadVMGObject(reader, input);
                            ExportTextures.TextureExportGCN(vmgobject);
                        }
                    }
                }
            }
        }
    }
}
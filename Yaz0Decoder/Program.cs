using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaz0Decoder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please enter the path to the Yaz0 encoded file:");
                args = new string[1];
                args[0] = Console.ReadLine();
            }
            if (File.Exists(args[0]))
            {
                byte[] Data = File.ReadAllBytes(args[0]);
                if (Encoding.ASCII.GetString(Data.Take(4).ToArray()) == "Yaz0")
                {
                    // Successfully deterimed it's a Yaz0 file.
                    int Decompressed_Size = BitConverter.ToInt32(Data.Skip(4).Take(4).Reverse().ToArray(), 0);
                    Data = Data.Skip(16).ToArray();
                    byte[] Decompressed_Data = new byte[Decompressed_Size];

                    int Read_Position = 0;
                    int Write_Position = 0;
                    uint ValidBitCount = 0;
                    byte CurrentCodeByte = 0;

                    while (Write_Position < Decompressed_Size)
                    {
                        if (ValidBitCount == 0)
                        {
                            CurrentCodeByte = Data[Read_Position];
                            ++Read_Position;
                            ValidBitCount = 8;
                        }

                        if ((CurrentCodeByte & 0x80) != 0)
                        {
                            Decompressed_Data[Write_Position] = Data[Read_Position];
                            Write_Position++;
                            Read_Position++;
                        }
                        else
                        {
                            byte Byte1 = Data[Read_Position];
                            byte Byte2 = Data[Read_Position + 1];
                            Read_Position += 2;

                            uint Dist = (uint)(((Byte1 & 0xF) << 8) | Byte2);
                            uint CopySource = (uint)(Write_Position - (Dist + 1));

                            uint Byte_Count = (uint)(Byte1 >> 4);
                            if (Byte_Count == 0)
                            {
                                Byte_Count = (uint)(Data[Read_Position] + 0x12);
                                Read_Position++;
                            }
                            else
                            {
                                Byte_Count += 2;
                            }

                            for (int i = 0; i < Byte_Count; ++i)
                            {
                                Decompressed_Data[Write_Position] = Decompressed_Data[CopySource];
                                CopySource++;
                                Write_Position++;
                            }
                        }

                        CurrentCodeByte <<= 1;
                        ValidBitCount -= 1;
                    }

                    string File_Type = "bin";
                    // Check to see if our decompressed file has an extension
                    if (Decompressed_Data[0] != 0)
                    {
                        File_Type = Encoding.ASCII.GetString(Decompressed_Data.Take(4).ToArray()).ToLower();
                    }
                    string File_Path = Path.GetDirectoryName(args[0]) + @"\" + Path.GetFileNameWithoutExtension(args[0]);
                    FileStream Decompressed_File;
                    try { Decompressed_File = File.Create(File_Path + @"." + File_Type, Decompressed_Size); }
                    catch { Decompressed_File = File.Create(File_Path + @".bin", Decompressed_Size); }
                    Decompressed_File.Write(Decompressed_Data, 0, Decompressed_Size);
                    Decompressed_File.Flush();
                    Decompressed_File.Close();
                    Console.WriteLine("Successfully decompressed Yaz0 compressed file!");
                    Console.WriteLine("Press any key to close the window.");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("The file does not appear to be a valid Yaz0 compressed file! Press any key to close the window.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("The file specified does not exist. Press any key to close the window.");
                Console.ReadKey();
            }
        }
    }
}

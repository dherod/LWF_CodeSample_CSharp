using System;
using System.Collections.Generic;
using System.Text;

namespace LWF_CodeSample_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Convert hex strings to UTF-8 strings

            // First hex string
            string hexstring1 = "5765204d616b652054756d6d69657320486170707921";            

            // More complex hex string
            string hexstring2 = "e299a5205765204d616b652054756d6d792048617070792120e299a5"; 

            // Version 1
            // Wrote this C# version BEFORE wrote Ruby version.
            // Traverses hex string character by character encoding to UTF-8 as it goes.
            // This consists of evaluating first bytes to determine number of bytes in sequence for each UTF-8 character
            // and accumulating and converting the byte sequences for each character, then adding to output string.
            Console.WriteLine(GetUTF8StringFromHexString(hexstring1) + "\n");
            Console.WriteLine(GetUTF8StringFromHexString(hexstring2) + "\n");

            // Version 2
            // Wrote this C# version AFTER wrote Ruby version.
            // Note this only calls PackHex().            
            // After learning the value of the Ruby pack method, this approach converts the entire string first
            // to an array of bytes, then UTF-8 encodes the entire array.
            Console.WriteLine(Encoding.UTF8.GetString(PackHex(hexstring1)) + "\n");
            Console.WriteLine(Encoding.UTF8.GetString(PackHex(hexstring2)) + "\n");
        }

        /// <summary>
        /// Port of Ruby pack method for converting strings to array of bytes
        /// </summary>
        /// <param name="hexstring">The whole string of hex being encoded</param>
        /// <returns>The input string converted to an array of bytes</returns>
        static byte[] PackHex(string hexstring)
        {
            List<byte> hexBytes = new List<byte>();
            for (int i = 0; i < hexstring.Length; i += 2)
            {
                string byteString = hexstring.Substring(i, 2);
                byte byteHex = Convert.ToByte(byteString, 16);
                hexBytes.Add(byteHex);
            }

            return hexBytes.ToArray();
        }

        /// <summary>
        /// Get UTF-8 string from a string of hex
        /// </summary>
        /// <param name="hexstring">The whole string of hex being encoded</param>
        /// <returns>The UTF-8 encoded string</returns>
        static string GetUTF8StringFromHexString(string hexstring)
        {
            try
            {
                // Holds string to output
                StringBuilder output = new StringBuilder();

                // Parse supplied string of hex as UTF-8 encoded characters
                for (int i = 0; i < hexstring.Length; i += 2)
                {
                    // Get the first byte of the next UTF-8 sequence
                    string byte1 = hexstring.Substring(i, 2);

                    // Get the number of bytes in the UTF-8 sequence based upon the first byte
                    int numBytes = GetNumberOfBytes(byte1);

                    // Get the UTF-8 sequence of bytes representing the next character
                    List<byte> currentSequence = GetSequence(hexstring, i, numBytes);

                    // Store the character that the UTF-8 sequence of bytes represents
                    output.Append(Encoding.UTF8.GetString(currentSequence.ToArray()));

                    // Adjust for the number of bytes in the UTF-8 sequence. 
                    // Note that the for loop is already advancing one byte per iteration.
                    i += (numBytes - 1) * 2;
                }

                // Return the UTF-8 encoded string
                return (output.ToString());
            }
            catch (Exception exc)
            {
                return string.Format("UTF-8 Encoding failed for {0}.\n{1} {2}", hexstring, exc.Message, exc.InnerException);
            }            
        }
        
        /// <summary>
        /// Get number of bytes for UTF-8 sequence based upon first byte in sequence
        /// Reference: http://www.fileformat.info/info/unicode/utf8.htm
        /// </summary>
        /// <param name="byte1">The first byte of sequence</param>
        /// <returns>The number of bytes in sequence</returns>
        static int GetNumberOfBytes(string byte1)
        {
            // Use value of first byte to determine how many bytes in UTF-8 sequence
            byte hexByte1 = Convert.ToByte(byte1, 16);
            int numBytes = 0;
            if (hexByte1 >= 0x00 && hexByte1 <= 0x7F)
            {
                // one byte sequence
                numBytes = 1;
            }
            else if (hexByte1 >= 0x80 && hexByte1 <= 0xBF)
            {
                // This is continuing byte of a multi-byte sequence and
                // shouldn't be in byte 1 of a properly formatted hex string
                throw new Exception(string.Format("GetNumberOfBytes() -> Input string is in an incorrect format: Byte {0}", byte1));
            }
            else if (hexByte1 >= 0xC2 && hexByte1 <= 0xDF)
            {
                // two byte sequence
                numBytes = 2;
            }
            else if (hexByte1 >= 0xE0 && hexByte1 <= 0xEF)
            {
                // three byte sequence
                numBytes = 3;
            }
            else if (hexByte1 >= 0xF0 && hexByte1 <= 0xFF)
            {
                // four byte sequence
                numBytes = 4;
            }

            return numBytes;
        }

        /// <summary>
        /// Get the UTF-8 sequence for the current character
        /// </summary>
        /// <param name="hexstring">The whole string being encoded</param>
        /// <param name="position">The current position in the whole string</param>
        /// <param name="numBytes">The number of bytes in the current character</param>
        /// <returns>The UTF-8 sequence for the current character as a list of bytes</returns>
        static List<byte> GetSequence(string hexstring, int position, int numBytes)
        {
            // Check input
            if (numBytes == 0 || string.IsNullOrWhiteSpace(hexstring))
                throw new Exception(string.Format("GetSequence() -> Invalid input: {0} {1}", numBytes, hexstring));

            // Holds current byte sequence for UTF-8 character
            List<byte> currentSequence = new List<byte>();

            // Add bytes to sequence
            for (int i = position; i < position + numBytes * 2; i += 2)
            {
                string byteString = hexstring.Substring(i, 2);
                byte byteHex = Convert.ToByte(byteString, 16);
                currentSequence.Add(byteHex);
            }

            return currentSequence;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco.Base
{
    public static class EncryptDecrypt
    {
		static private readonly string FLAG_ENCRYPTION = "[shaco_secret_flag]";
        static private readonly string FLAG_ENCRYPTION_PARAM_BEGIN = "[shaco_secret_param_begin]";
        static private readonly string FLAG_ENCRYPTION_PARAM_END = "[shaco_secret_param_end]";

        //跳转速度下标
        static private readonly int JUMP_SPEED_PARAMETER_INDEX = 0;

        //加密码下标
        static private readonly int SECRET_CODE_PARAMETER_INDEX = 1;

        //自定义字段开始下标
        static private readonly int CUSTOM_PARAMETER_START_INDEX = 2;

        //参数分割符
        static private readonly string PARAMETER_SPLIT_FLAG = ",";

        /// <summary>
        /// encrypt the  and overwrite
        /// </summary>
        /// <param name ="path">文件路径</param>
        /// <param name ="jumpSpeed">当范围(0 ~ 1)按百分比进行跳转速度加密。当范围(大于等于1)按固定整数数量跳转速度加密</param>
        /// <param name ="secretCode">加密字符</param>
        static public void Encrypt(string path, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            var readBytesTmp = FileHelper.ReadAllByteByUserPath(path);

            if (readBytesTmp.IsNullOrEmpty())
            {
                return;
            }

			var extensionsTmp = FileHelper.GetFilNameExtension(path);

            var encryptBytes = Encrypt(readBytesTmp, jumpSpeed, secretCode, customParameters);
            if (null != encryptBytes)
            {
                try
                {
                    //overwrite
                    path = FileHelper.RemoveExtension(path);
                    if (!string.IsNullOrEmpty(extensionsTmp))
                    {
                        path = FileHelper.AddExtensions(path, extensionsTmp);
                    }
                    FileHelper.WriteAllByteByUserPath(path, encryptBytes);
                }
                catch (System.Exception e)
                {
                    Log.Error("HotUpdate Encrypt errror: overwrite \nexception=" + e);
                }
            }
        }

        static public byte[] Encrypt(byte[] bytes, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            byte[] retValue = null;

            if (bytes.IsNullOrEmpty())
                return bytes;

            if (IsEncryption(bytes))
            {
                Log.Error("HotUpdate Encrypt error: has been encrypted, byte length=" + bytes.Length);
                return retValue;
            }

            var encryptionString = GetEncryptString(jumpSpeed, secretCode, customParameters);
            retValue = new byte[bytes.Length + encryptionString.Length];

            //------------------------------------------------------------
            //encrypting
            //------------------------------------------------------------

            try
            {
                //set encryption flag
                for (int i = 0; i < encryptionString.Length; ++i)
                {
                    retValue[i] = (byte)encryptionString[i];
                }
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate Encrypt errror: set encryption flag \nexception=" + e);
                return retValue;
            }

            try
            {
                //set encryption bytes
                bytes = EncryptOrDecrypt(bytes, jumpSpeed, secretCode);
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate Encrypt errror: set encryption bytes \nexception=" + e);
                return retValue;
            }

            try
            {
                System.Array.Copy(bytes, 0, retValue, encryptionString.Length, bytes.Length);
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate Encrypt errror: copy encrypt string \nexception=" + e);
                return retValue;
            }
            return retValue;
        }

        /// <summary>
        /// decrypt the and read orignial bytes from file
        /// </summary>
        /// <param name ="path">file path</param>
        static public byte[] Decrypt(string path)
        {
            byte[] readBytesTmp = null;
            if (!FileHelper.ExistsFile(path))
            {
                return readBytesTmp;
            }
            readBytesTmp = FileHelper.ReadAllByteByUserPath(path);
            return Decrypt(readBytesTmp);
        }

        static public byte[] Decrypt(byte[] bytes)
        {
            if (!IsEncryption(bytes))
            {
                return bytes;
            }

            //------------------------------------------------------------
            //decrypting
            //------------------------------------------------------------

            var paramsTmp = GetEncryptParameters(bytes);
            float? jumpSpeed = null;
            long? secretCode = null;

            //jump length
            if (paramsTmp.Length > JUMP_SPEED_PARAMETER_INDEX && !string.IsNullOrEmpty(paramsTmp[JUMP_SPEED_PARAMETER_INDEX]))
            {
                jumpSpeed = paramsTmp[JUMP_SPEED_PARAMETER_INDEX].ToFloat();
            }
            //encrypt or decrypt code
            if (paramsTmp.Length > SECRET_CODE_PARAMETER_INDEX && !string.IsNullOrEmpty(paramsTmp[SECRET_CODE_PARAMETER_INDEX]))
            {
                secretCode = paramsTmp[SECRET_CODE_PARAMETER_INDEX].ToLong();
            }

            int encryptionStringLength = 0;

            //Compatibility of old versions of decryption
            if (jumpSpeed == null && secretCode == null)
            {
                jumpSpeed = 14;
                secretCode = 36;
            }
            else
            {
                encryptionStringLength = GetEncryptStringLength(bytes);
            }

            long newBytesCount = bytes.Length - encryptionStringLength;
            if (newBytesCount <= 0)
            {
                Log.Error("HotUpdate Decrypt error: invalid bytes count=" + newBytesCount);
                bytes = null;
                return bytes;
            }

            var encryptBytes = new byte[newBytesCount];
            try
            {
                //remove encryption flag
                System.Array.Copy(bytes, encryptionStringLength, encryptBytes, 0, newBytesCount);
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate Decrypt errror: remove encryption flag \nexception=" + e);
                encryptBytes = null;
                return encryptBytes;
            }

            try
            {
                //set decryption bytes
                encryptBytes = EncryptOrDecrypt(encryptBytes, jumpSpeed, secretCode);
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate Decrypt errror: set decryption bytes \nexception=" + e);
                encryptBytes = null;
                return encryptBytes;
            }

            return encryptBytes;
        }

        /// <summary>
        /// encrypt the and overwrite with Asynchronous
        /// </summary>
        /// <param name ="path">file path</param>
        /// <param name ="callbackEnd">completed callback function</param>
        /// <param name ="jumpSpeed">encryt and decrypt speed, range(0 ~ 1)</param>
        static public void EncryptAsync(string path, System.Action callbackEnd, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            shaco.Base.ThreadPool.RunThreadSafeCallBack(()=>
            {
                Encrypt(path, jumpSpeed, secretCode, customParameters);
            }, callbackEnd);
        }

        static public void EncryptAsync(byte[] bytes, System.Action callbackEnd, float jumpSpeed = 0.5f, long secretCode = 36, params object[] customParameters)
        {
            shaco.Base.ThreadPool.RunThreadSafeCallBack(() =>
            {
                Encrypt(bytes, jumpSpeed, secretCode, customParameters);
            }, callbackEnd);
        }

        /// <summary>
        /// encrypt the and overwrite with Asynchronous
        /// </summary>
        /// <param name ="path">file path</param>
        /// <param name ="callbackEnd">completed callback function</param>
        static public void DecryptAsync(string path, System.Action<byte[]> callbackEnd)
        {
            DecryptAsyncBase(()=>
            {
                return Decrypt(path);
            }, callbackEnd);
        }

        static public void DecryptAsync(byte[] bytes, System.Action<byte[]> callbackEnd)
        {
            DecryptAsyncBase(() =>
            {
                return Decrypt(bytes);
            }, callbackEnd);
        }

        static public bool IsEncryption(string path)
        {
            if (shaco.Base.FileHelper.ExistsFile(path))
            {
                return IsEncryption(GetEncryptHeader(path));
            }
            else 
            {
                return false;
            }
        }

        static public bool IsEncryption(byte[] bytes)
        {
            bool retValue = false;

            if (null == bytes || bytes.Length == 0)
            {
                return retValue;
            }

            if (bytes.Length < EncryptDecrypt.FLAG_ENCRYPTION.Length)
            {
                // Log.Error("HotUpdate IsEncryptiond error: There is not enough length to verify");
                return retValue;
            }

            retValue = BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION);
            return retValue;
        }

        /// <summary>
        /// 获取用户自定义字段，如果没有则返回null
        /// <param name="bytes">二进制数组</param>
        /// <return>自定义字段数组</return>
        /// </summary>
        static public string[] GetCustomParameters(byte[] bytes)
        {
            string[] retValue = new string[0];
            if (null == bytes || bytes.Length == 0)
            {
                return retValue;
            }

            var parameters = GetEncryptParameters(bytes);
            if (null != parameters && parameters.Length > CUSTOM_PARAMETER_START_INDEX)
            {
                retValue = new string[parameters.Length - CUSTOM_PARAMETER_START_INDEX];
                for (int i = CUSTOM_PARAMETER_START_INDEX; i < parameters.Length; ++i)
                {
                    retValue[i - CUSTOM_PARAMETER_START_INDEX] = parameters[i];
                }
            }

            return retValue;
        }

        /// <summary>
        /// 获取用户自定义字段，如果没有则返回null
        /// <param name="path">文件绝对路径</param>
        /// <return>自定义字段数组</return>
        /// </summary>
        static public string[] GetCustomParameters(string path)
        {
            return GetCustomParameters(GetEncryptHeader(path));
        }

        /// <summary>
        /// 获取文件加密跳转速度
        /// <param name="path">文件绝对路径</param>
        /// <return>加密跳转速度，如果获取失败返回0</return>
        /// </summary>
        static public float GetJumpSpeed(string path)
        {
            var parameters = GetEncryptParameters(GetEncryptHeader(path));
            if (!parameters.IsNullOrEmpty() && parameters.Length > JUMP_SPEED_PARAMETER_INDEX)
            {
                return parameters[JUMP_SPEED_PARAMETER_INDEX].ToFloat();
            }
            else 
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// 获取文件加密码
        /// <param name="path">文件绝对路径</param>
        /// <return>加密码，如果获取失败返回0</return>
        /// </summary>
        static public long GetSecretCode(string path)
        {
            var parameters = GetEncryptParameters(GetEncryptHeader(path));
            if (!parameters.IsNullOrEmpty() && parameters.Length > SECRET_CODE_PARAMETER_INDEX)
            {
                return parameters[SECRET_CODE_PARAMETER_INDEX].ToInt();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取加密开头内容
        /// <param name="path">文件绝对路径</param>
        /// <return>加密二进制文件</return>
        /// </summary>
        static private byte[] GetEncryptHeader(string path)
        {
            byte[] retValue = null;
            System.IO.FileStream fileStream = null;
            System.IO.BinaryReader binaryReader = null;

            try
            {
                if (!shaco.Base.FileHelper.ExistsFile(path))
                {
                    return retValue;
                }

                fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                binaryReader = new System.IO.BinaryReader(fileStream);
                var readOffset = FLAG_ENCRYPTION.Length * 2;
                var readBuffer = new List<byte>();
                int currentReadIndex = 0;
                var currentReadBuffer = new byte[readOffset];

                currentReadIndex = binaryReader.Read(currentReadBuffer, 0, currentReadBuffer.Length);

                //判断是否为加密文件
                if (!BeginContains(currentReadBuffer, FLAG_ENCRYPTION))
                {
                    return retValue;
                }

                readBuffer.AddRange(currentReadBuffer);

                while (true)
                {
                    currentReadIndex = binaryReader.Read(currentReadBuffer, 0, currentReadBuffer.Length);

                    //没有可读文件，退出循环
                    if (currentReadIndex <= 0)
                        break;

                    //添加本次读取到的内容
                    readBuffer.AddRange(currentReadBuffer);

                    if (Contains(readBuffer.ToArray(), FLAG_ENCRYPTION_PARAM_END))
                    {
                        break;
                    }
                }
                retValue = readBuffer.ToArray();
            }
            catch (System.Exception e)
            {
                shaco.Log.Error("EncryptAndDecrypt GetEncryptHeader exception=" + e);
            }
            finally
            {
                if (null != binaryReader)
                {
                    binaryReader.Close();
                }
                if (null != fileStream)
                {
                    fileStream.Close();
                }
            }
            return retValue;
        }

        static private void DecryptAsyncBase(System.Func<byte[]> callbackDecrypt, System.Action<byte[]> callbackEnd)
        {
            bool isCompleted = false;
            byte[] bytesDecrypt = null;
            System.Threading.ThreadStart funcThread = () =>
            {
                bytesDecrypt = callbackDecrypt();
                isCompleted = true;
            };

            var thread = new System.Threading.Thread(funcThread);
            thread.Start();

            shaco.Base.WaitFor.Run(() =>
            {
                return isCompleted;
            }, () =>
            {
                if (null != callbackEnd)
                {
                    callbackEnd(bytesDecrypt);
                }
            });
        }

        /// <summary>
        /// 获取加密配置长度
        /// <param name="bytes">二进制内容</param>
        /// <return>加密配置长度</return>
        /// </summary>
        static private int GetEncryptStringLength(byte[] bytes)
        {
            int retValue = 0;
            if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN, EncryptDecrypt.FLAG_ENCRYPTION.Length))
            {
                var paramsString = string.Empty;
                var offsetIndex = EncryptDecrypt.FLAG_ENCRYPTION.Length + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN.Length;

                for (int i = offsetIndex; i < bytes.Length; ++i)
                {
                    if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END, i))
                    {
                        retValue = i + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END.Length;
                        break;
                    }
                    else
                    {
                        paramsString += (char)(bytes[i] ^ GlobalParams.ENCRYPT_SECRET_CODE);
                    }
                }
            }
            return retValue;
        }

        static public string GetEncryptString(float? jumpSpeed, long? secretCode, object[] customParameters)
        {
            var encryptionString = new System.Text.StringBuilder();
            encryptionString.Append(EncryptDecrypt.FLAG_ENCRYPTION + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN);

            //add custom encrypt parameters 
            if (null != jumpSpeed)
            {
                encryptionString.Append(jumpSpeed.ToString());
                encryptionString.Append(PARAMETER_SPLIT_FLAG);
            }
            if (null != secretCode)
            {
                encryptionString.Append(secretCode.ToString());
                encryptionString.Append(PARAMETER_SPLIT_FLAG);
            }
            if (null != customParameters && customParameters.Length > 0)
            {
                for (int i = 0; i < customParameters.Length; ++i)
                {
                    encryptionString.Append(customParameters[i].ToString());
                    encryptionString.Append(PARAMETER_SPLIT_FLAG);
                }
            }

            //删除最后一个分隔符
            if (encryptionString.Length > 0)
            {
                encryptionString.Remove(encryptionString.Length - PARAMETER_SPLIT_FLAG.Length, PARAMETER_SPLIT_FLAG.Length);
            }

            encryptionString.Append(EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END);
            return EncryptOrDecrypt(encryptionString.ToString().ToByteArray(), 0, GlobalParams.ENCRYPT_SECRET_CODE).ToStringArray();
        }

        static private byte[] EncryptOrDecrypt(byte[] bytes, float? jumpSpeed, long? secretCode)
        {
            if (null == jumpSpeed || null == secretCode)
            {
                Log.Error("HotUpdateHelper EncryptOrDecrypt error: invalid param, jumpSpeed=" + jumpSpeed + " secretCode=" + secretCode);
                return bytes;
            }

            int addCount = 1;

            //Compatibility of old versions of decryption
            if (jumpSpeed == 14)
            {
                addCount = bytes.Length / (int)jumpSpeed;
            }
            else
            {
                //为1表示文件内容不做加密，保留加密信息头
                if (jumpSpeed == 1)
                {
                    return bytes;
                }
                else if (jumpSpeed > 1)
                {
                    addCount = (int)jumpSpeed;
                }
                else 
                {
                    addCount = (int)(bytes.Length * jumpSpeed);
                }
            }

            if (addCount <= 0)
                addCount = 1;

            for (int i = 0; i < bytes.Length; i += addCount)
            {
                if (i >= bytes.Length)
                    break;

                bytes[i] = (byte)((char)bytes[i] ^ secretCode);
            }
            return bytes;
        }

        static private string[] GetEncryptParameters(byte[] bytes)
        {
            var retValue = new string[0];
            if (!IsEncryption(bytes))
            {
                return retValue;
            }

            if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN, EncryptDecrypt.FLAG_ENCRYPTION.Length))
            {
                var paramsString = string.Empty;
                var offsetIndex = EncryptDecrypt.FLAG_ENCRYPTION.Length + EncryptDecrypt.FLAG_ENCRYPTION_PARAM_BEGIN.Length;

                for (int i = offsetIndex; i < bytes.Length; ++i)
                {
                    if (BeginContains(bytes, EncryptDecrypt.FLAG_ENCRYPTION_PARAM_END, i))
                    {
                        retValue = paramsString.Split(PARAMETER_SPLIT_FLAG);
                        break;
                    }
                    else
                    {
                        paramsString += (char)(bytes[i] ^ GlobalParams.ENCRYPT_SECRET_CODE);
                    }
                }
            }
            return retValue;
        }

        static private bool BeginContains(byte[] bytes, string find, int startIndex = 0)
        {
            string text = "";
            for (int i = 0; i < find.Length; ++i)
            {
                if (i + startIndex < bytes.Length)
                {
                    var char1 = (char)bytes[i + startIndex];
                    var char2 = (char)find[i];
                    var char3 = (char)(char2 ^ GlobalParams.ENCRYPT_SECRET_CODE);
                    if (char1 != char2 && char1 != char3)
                    {
                        return false;
                    }
                    else 
                    {
                        text += char1 ^ GlobalParams.ENCRYPT_SECRET_CODE;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        static private bool Contains(byte[] bytes, string find, int startIndex = 0)
        {
            var convertString = new System.Text.StringBuilder();
            for (int i = startIndex; i < bytes.Length; ++i)
            {
                convertString.Append((char)(bytes[i] ^ GlobalParams.ENCRYPT_SECRET_CODE));
            }

            return convertString.ToString().Contains(find);
        }
    }
}	
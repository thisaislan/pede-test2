using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Thisaislan.PersistenceEasyToDeleteInEditor.Editor.Constants;
using UnityEditor;
using UnityEngine;

namespace Thisaislan.PersistenceEasyToDeleteInEditor.Editor.ScriptableObjects.Data
{
    [
        CreateAssetMenu(
            fileName = Metadata.DataFileName,
            menuName = Metadata.AssetMenuDataName,
            order = Metadata.AssetMenuDataOrder
        )
    ]
    internal class PedeData : ScriptableObject
    {
        
        [Serializable]
        internal struct Data
        {
            [SerializeField]
            internal string key;
            
            [SerializeField]
            internal string type;
            
            [SerializeField]
            [TextArea(Metadata.TextAreaDataMinLines, Metadata.TextAreaDataMaxLines)]
            internal string value;
            
            internal bool IsKeyNull() =>
                key == null;
            
            internal bool IsSameValue(string key, string type) =>
                this.key.Equals(key) && this.type.Equals(type);
        }
        
        [SerializeField]
        internal List<Data> playerPrefData = new List<Data>();
        
        [SerializeField]
        internal List<Data> fileData = new List<Data>();
        
        #region PlayerPrefsRegion
        
        internal void SetPlayerPrefs<T>(string key, T value)
        {
            CheckKeyAsNull(key);
            CheckValueAsNull(value);
            
            var data = GetFirstPlayerPrefsDataOrDefault<T>(key);

            if (!data.IsKeyNull())
            {
                var index = playerPrefData.IndexOf(data);
                
                playerPrefData.Remove(data);
                playerPrefData.Insert(index, CreatePlayerPrefsData(key, value));
            }
            else
            {
                playerPrefData.Add(CreatePlayerPrefsData(key, value));
            }

            PersistAsset();
        }

        internal void GetPlayerPrefs<T>(
            string key,
            Action<T> actionIfHasResult,
            Action actionIfHasNotResult = null,
            bool destroyAfter = false)
        {
            CheckKeyAsNull(key);
            CheckActionAsNull(actionIfHasResult);
            
            var data = GetFirstPlayerPrefsDataOrDefault<T>(key);

            if (!data.IsKeyNull())
            {
                GetPlayerPrefsData(data.value,  actionIfHasResult);
                
                if (destroyAfter) { DeletePlayerPrefs<T>(key); }
            }
            else
            {
                actionIfHasNotResult?.Invoke();
            }
        }

        internal void DeletePlayerPrefs<T>(string key)
        {
            RemovePlayerPrefsData<T>(key);
            PersistAsset();
        }
        
        internal void DeleteAllPlayerPrefs()
        {
            playerPrefData.Clear();
            PersistAsset();
        }

        internal void HasPlayerPrefsKey<T>(string key, Action<bool> actionWithResult)
        {
            CheckKeyAsNull(key);
            CheckActionAsNull(actionWithResult);
            
            actionWithResult.Invoke(ExistsData(playerPrefData, key, GetTypeName(typeof(T))));
        }

        private Data CreatePlayerPrefsData<T>(string key, T value) =>
            new Data
            {
                key = key,
                type = GetTypeName(typeof(T)),
                value = GetPlayerPrefsValue(value)
            };
        
        private string GetPlayerPrefsValue<T>(T value)
        {
            if (Metadata.BuildInTypes.Contains(typeof(T)))
            {
                return Convert.ToString(value);
            }
            else if (typeof(T) == typeof(nint))
            {
                return Convert.ToString(value);
            }
            else if (typeof(T) == typeof(nuint))
            {
                return Convert.ToString(value);
            }
            else
            {
                return JsonUtility.ToJson(value, true);
            }
        }

        private void GetPlayerPrefsData<T>(string value, Action<T> actionWithResult)
        {
            if (Metadata.BuildInTypes.Contains(typeof(T)))
            {
                GetBuildInTypePlayerPrefs(value, actionWithResult);
            }
            else if (typeof(T) == typeof(nint))
            {
                GetNintPlayerPrefs(Convert.ToInt32(value), actionWithResult as Action<nint>);
            }
            else if (typeof(T) == typeof(nuint))
            {
                GetUnintPlayerPrefs(Convert.ToUInt32(value), actionWithResult as Action<nuint>);
            }
            else
            {
                GetObject(JsonUtility.FromJson<T>(value), actionWithResult);
            }
        }

        private void GetBuildInTypePlayerPrefs<T>(string value, Action<T> actionWithResult) =>
            actionWithResult.Invoke(GetConvertedBuildInType<T>(value));

        private T GetConvertedBuildInType<T>(string value)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            return (T)typeConverter.ConvertFromString(value);
        }

        private void GetNintPlayerPrefs(nint value, Action<nint> actionWithResult) =>
            actionWithResult.Invoke(value);

        private void GetUnintPlayerPrefs(nuint value, Action<nuint> actionWithResult) =>
            actionWithResult.Invoke(value);

        private Data GetFirstPlayerPrefsDataOrDefault<T>(string key) =>
            GetFirstDataOrDefault(playerPrefData, key, GetTypeName(typeof(T)));

        private void RemovePlayerPrefsData<T>(string key) =>
            RemoveData(playerPrefData, key, GetTypeName(typeof(T)));
        
        private bool IsPlayerPrefsDataValuesValid(ValidationErrorHandler validationErrorHandler)
        {
            var dataIsValid = true;

            for (int index = 0; index < playerPrefData.Count; index++)
            {
                var data = playerPrefData[index];

                if (!string.IsNullOrEmpty(data.type))
                {
                      try 
                      {
                        var type = Metadata.BuildInTypes.FirstOrDefault(type => GetTypeName(type).Equals(data.type));
                        
                        if (type != default)
                        {
                            if (type == typeof(bool)) { GetConvertedBuildInType<bool>(data.value); } 
                            else if (type == typeof(byte)) { GetConvertedBuildInType<byte>(data.value); } 
                            else if (type == typeof(sbyte)) { GetConvertedBuildInType<sbyte>(data.value); } 
                            else if (type == typeof(char)) { GetConvertedBuildInType<char>(data.value); } 
                            else if (type == typeof(decimal)) { GetConvertedBuildInType<decimal>(data.value); } 
                            else if (type == typeof(double)) { GetConvertedBuildInType<double>(data.value); } 
                            else if (type == typeof(float)) { GetConvertedBuildInType<float>(data.value); } 
                            else if (type == typeof(int)) { GetConvertedBuildInType<int>(data.value); } 
                            else if (type == typeof(uint)) { GetConvertedBuildInType<uint>(data.value); } 
                            else if (type == typeof(long)) { GetConvertedBuildInType<long>(data.value); } 
                            else if (type == typeof(ulong)) { GetConvertedBuildInType<ulong>(data.value); } 
                            else if (type == typeof(short)) { GetConvertedBuildInType<short>(data.value); } 
                            else if (type == typeof(ushort)) { GetConvertedBuildInType<ushort>(data.value); } 
                            else if (type == typeof(string)) { GetConvertedBuildInType<string>(data.value); }
                        }
                        else if (data.type == GetTypeName(typeof(nint))) { Convert.ToInt32(data.value); }
                        else if (data.type == GetTypeName(typeof(nuint))) { Convert.ToUInt32(data.value); }
                        else { JsonUtility.FromJson<object>(data.value); }
                      }
                      catch
                      {
                          HasError(data.key, index);
                      }
                }
                else
                {
                    HasError(data.key, index);
                }
            }
            
            void HasError(string keyInValidation, int index)
            {
                dataIsValid = false;
                validationErrorHandler.HandleValueError(keyInValidation, index, false);
            }

            return dataIsValid;
        }

        private bool IsPlayerPrefsDataKeysValid(ValidationErrorHandler validationErrorHandler) =>
            IsKeysValid(playerPrefData, validationErrorHandler, false);

        private bool IsPlayerPrefsDataTypesValid(ValidationErrorHandler validationErrorHandler) =>
            IsTypesValid(playerPrefData, validationErrorHandler, false);

        #endregion //PlayerPrefsRegion
        
        #region FileRegion
        
        internal void SetFile<T>(string key, T value)
        {
            CheckKeyAsNull(key);
            CheckValueAsNull(value);
            
            var data = GetFirstFileDataOrDefault<T>(key);

            if (!data.IsKeyNull())
            {
                var index = fileData.IndexOf(data);
                
                fileData.Remove(data);
                fileData.Insert(index, CreateFileData(key, value));
            }
            else
            {
                fileData.Add(CreateFileData(key, value));
            }

            PersistAsset();
        }
        
        internal void GetFile<T>(
            string key,
            Action<T> actionIfHasResult,
            Action actionIfHasNotResult = null,
            bool destroyAfter = false)
        {
            CheckKeyAsNull(key);
            CheckActionAsNull(actionIfHasResult);
            
            var data = GetFirstFileDataOrDefault<T>(key);

            if (!data.IsKeyNull())
            {
                GetFileData(data.value,  actionIfHasResult);
                
                if (destroyAfter) { DeleteFile<T>(key); }
            }
            else
            {
                actionIfHasNotResult?.Invoke();
            }
        }
        
        internal void DeleteFile<T>(string key)
        {
            CheckKeyAsNull(key);
            RemoveFile<T>(key);
            PersistAsset();
        }
        
        internal void DeleteAllFiles()
        {
            fileData.Clear();
            PersistAsset();
        }

        internal void  HasFileKey<T>(string key, Action<bool> actionWithResult)
        {
            CheckKeyAsNull(key);
            CheckActionAsNull(actionWithResult);
            
            actionWithResult.Invoke(ExistsData(fileData, key, GetTypeName(typeof(T))));
        }

        private void RemoveFile<T>(string key) =>
            RemoveData(fileData, key, GetTypeName(typeof(T)));
        
        private void GetFileData<T>(string value, Action<T> actionWithResult) =>
            GetObject(JsonUtility.FromJson<T>(value), actionWithResult);

        private Data GetFirstFileDataOrDefault<T>(string key) =>
            GetFirstDataOrDefault(fileData, key, GetTypeName(typeof(T)));
        
        private Data CreateFileData<T>(string key, T value) =>
            new Data
            {
                key = key,
                type = GetTypeName(typeof(T)),
                value =  JsonUtility.ToJson(value, true)
            };
        
        private bool IsFileDataValuesValid(ValidationErrorHandler validationErrorHandler)
        {
            var dataIsValid = true;

            for (int index = 0; index < fileData.Count; index++)
            {
                var data = fileData[index];

                if (!string.IsNullOrEmpty(data.type))
                {
                    try { JsonUtility.FromJson<object>(data.value); }
                    catch { HasError(data.key, index); }
                }
                else
                {
                    HasError(data.key, index);
                }
            }

            void HasError(string keyInValidation, int index)
            {
                dataIsValid = false;
                validationErrorHandler.HandleValueError(keyInValidation, index, true);
            }
            
            return dataIsValid;
        }
        
        private bool IsFileDataKeysValid(ValidationErrorHandler validationErrorHandler) =>
            IsKeysValid(fileData, validationErrorHandler, true);
        
        private bool IsFileDataTypesValid(ValidationErrorHandler validationErrorHandler) =>
            IsTypesValid(fileData, validationErrorHandler, false);
        
        #endregion //FileRegion

        #region UtilsRegion

        internal void DeleteAll()
        {
            DeleteAllPlayerPrefs();
            DeleteAllFiles();
        }

        internal bool IsDataValid(ValidationErrorHandler validationErrorHandler)
        {
            var isPlayerPrefsDataTypesValid = IsPlayerPrefsDataTypesValid(validationErrorHandler);
            var isFileDataTypesValid = IsFileDataTypesValid(validationErrorHandler);
            
            var isPlayerPrefsValuesValid = IsPlayerPrefsDataValuesValid(validationErrorHandler);
            var isFileDataValuesValid = IsFileDataValuesValid(validationErrorHandler);
            
            var isPlayerPrefsKeysValid = IsPlayerPrefsDataKeysValid(validationErrorHandler);
            var isFileDataKeysValid = IsFileDataKeysValid(validationErrorHandler);
            

            return isPlayerPrefsValuesValid &&
                   isFileDataValuesValid &&
                   isPlayerPrefsKeysValid &&
                   isFileDataKeysValid &&
                   isPlayerPrefsDataTypesValid &&
                   isFileDataTypesValid;
        }

        private bool IsKeysValid(List<Data> dataList, ValidationErrorHandler validationErrorHandler, bool isFileData)
        {
            var dataIsValid = true;

            for (int index = 0; index < dataList.Count; index++)
            {
                var data = dataList[index];

                if (string.IsNullOrEmpty(data.key))
                {
                    validationErrorHandler.HandleKeyError(data.value, index, isFileData, false);
                    dataIsValid = false;
                }

                if (IsDuplicatedKey(data.key, dataList))
                {
                    validationErrorHandler.HandleKeyError(data.value, index, isFileData, true);
                    dataIsValid = false;
                }
            }

            return dataIsValid;
        }

        private bool IsDuplicatedKey(string key, List<Data> dataList) =>
            dataList.FindAll(innerData => innerData.key == key).Count > 1;
        
        private bool IsTypesValid(List<Data> dataList, ValidationErrorHandler validationErrorHandler, bool isFileData)
        {
            var dataIsValid = true;

            for (int index = 0; index < dataList.Count; index++)
            {
                var data = dataList[index];

                if (string.IsNullOrEmpty(data.type))
                {
                    validationErrorHandler.HandleTypeError(data.value, index, isFileData);
                    dataIsValid = false;
                }
            }

            return dataIsValid;
        }

        private void GetObject<T>(T value, Action<T> actionIfHasResult) =>
            actionIfHasResult.Invoke(value);
        
        private bool ExistsData(List<Data> dataList, string key, string typeName) =>
            dataList.Exists(data => data.IsSameValue(key, typeName));

        private void RemoveData(List<Data> dataList, string key, string typeName) =>
            dataList.RemoveAll(data => data.IsSameValue(key, typeName));

        private Data GetFirstDataOrDefault(List<Data> daraList, string key, string typeName) =>
            daraList.FirstOrDefault(data => data.IsSameValue(key, typeName));

        private string GetTypeName(Type type) =>
            type.ToString();
        
        private void CheckKeyAsNull(string key) =>
            CheckArgumentAsNull(key, nameof(key));
        
        private void CheckValueAsNull<T>(T value) =>
            CheckArgumentAsNull(value, nameof(value));
        
        private void CheckActionAsNull<T>(Action<T> actionIfHasResult) =>
            CheckArgumentAsNull(actionIfHasResult, nameof(actionIfHasResult));
        
        private void CheckArgumentAsNull<T>(T argument, string argumentName)
        {
            if (argument == null) { throw new ArgumentNullException(nameof(argumentName)); }
        }

        private void PersistAsset()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion //UtilsRegion
        
        internal class ValidationErrorHandler
        {
            private readonly Action<string, int, bool> ActionOnValidationIndividualValueError;
            private readonly Action<string, int, bool, bool> ActionOnValidationIndividualKeyError;
            private readonly Action<string, int, bool> ActionOnValidationIndividualTypeError;

            internal ValidationErrorHandler(
                Action<string, int, bool> actionOnValidationIndividualValueError,
                Action<string, int, bool, bool> actionOnValidationIndividualKeyError,
                Action<string, int, bool> actionOnValidationIndividualTypeError
            )
            {
                this.ActionOnValidationIndividualValueError = actionOnValidationIndividualValueError;
                this.ActionOnValidationIndividualKeyError = actionOnValidationIndividualKeyError;
                this.ActionOnValidationIndividualTypeError = actionOnValidationIndividualTypeError;
            }

            internal void HandleValueError(string key, int index, bool isFileData) =>
                ActionOnValidationIndividualValueError.Invoke(key, index, isFileData);
            
            internal void HandleKeyError(string key, int index, bool isFileData, bool isDuplicity) =>
                ActionOnValidationIndividualKeyError.Invoke(key, index, isFileData, isDuplicity);
            
            internal void HandleTypeError(string key, int index, bool isFileData) =>
                ActionOnValidationIndividualTypeError.Invoke(key, index, isFileData);

        }

    }
}
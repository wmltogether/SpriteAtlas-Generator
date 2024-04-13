namespace SpriteAtlasGenerator.Editor.SimpleJSON
{
    using System.Globalization;
    using System.Collections.Generic;
    public partial class JSONNode
    {
        #region Decimal
        public virtual decimal AsDecimal
        {
            get
            {
                decimal result;
                if (!decimal.TryParse(Value, out result))
                    result = 0;
                return result;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static implicit operator JSONNode(decimal aDecimal)
        {
            return new JSONString(aDecimal.ToString(CultureInfo.InvariantCulture));
        }

        public static implicit operator decimal(JSONNode aNode)
        {
            return aNode.AsDecimal;
        }
        #endregion Decimal

        #region Char
        public virtual char AsChar
        {
            get
            {
                if (IsString && Value.Length > 0)
                    return Value[0];
                if (IsNumber)
                    return (char)AsInt;
                return '\0';
            }
            set
            {
                if (IsString)
                    Value = value.ToString();
                else if (IsNumber)
                    AsInt = (int)value;
            }
        }

        public static implicit operator JSONNode(char aChar)
        {
            return new JSONString(aChar.ToString());
        }

        public static implicit operator char(JSONNode aNode)
        {
            return aNode.AsChar;
        }
        #endregion Decimal

        #region UInt
        public virtual uint AsUInt
        {
            get
            {
                return (uint)AsDouble;
            }
            set
            {
                AsDouble = value;
            }
        }

        public static implicit operator JSONNode(uint aUInt)
        {
            return new JSONNumber(aUInt);
        }

        public static implicit operator uint(JSONNode aNode)
        {
            return aNode.AsUInt;
        }
        #endregion UInt

        #region Byte
        public virtual byte AsByte
        {
            get
            {
                return (byte)AsInt;
            }
            set
            {
                AsInt = value;
            }
        }

        public static implicit operator JSONNode(byte aByte)
        {
            return new JSONNumber(aByte);
        }

        public static implicit operator byte(JSONNode aNode)
        {
            return aNode.AsByte;
        }
        #endregion Byte
        #region SByte
        public virtual sbyte AsSByte
        {
            get
            {
                return (sbyte)AsInt;
            }
            set
            {
                AsInt = value;
            }
        }

        public static implicit operator JSONNode(sbyte aSByte)
        {
            return new JSONNumber(aSByte);
        }

        public static implicit operator sbyte(JSONNode aNode)
        {
            return aNode.AsSByte;
        }
        #endregion SByte

        #region Short
        public virtual short AsShort
        {
            get
            {
                return (short)AsInt;
            }
            set
            {
                AsInt = value;
            }
        }

        public static implicit operator JSONNode(short aShort)
        {
            return new JSONNumber(aShort);
        }

        public static implicit operator short(JSONNode aNode)
        {
            return aNode.AsShort;
        }
        #endregion Short
        #region UShort
        public virtual ushort AsUShort
        {
            get
            {
                return (ushort)AsInt;
            }
            set
            {
                AsInt = value;
            }
        }

        public static implicit operator JSONNode(ushort aUShort)
        {
            return new JSONNumber(aUShort);
        }

        public static implicit operator ushort(JSONNode aNode)
        {
            return aNode.AsUShort;
        }
        #endregion UShort

        #region DateTime
        public virtual System.DateTime AsDateTime
        {
            get
            {
                System.DateTime result;
                if (!System.DateTime.TryParse(Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    result = new System.DateTime(0);
                return result;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static implicit operator JSONNode(System.DateTime aDateTime)
        {
            return new JSONString(aDateTime.ToString(CultureInfo.InvariantCulture));
        }

        public static implicit operator System.DateTime(JSONNode aNode)
        {
            return aNode.AsDateTime;
        }
        #endregion DateTime
        #region TimeSpan
        public virtual System.TimeSpan AsTimeSpan
        {
            get
            {
                System.TimeSpan result;
                if (!System.TimeSpan.TryParse(Value, CultureInfo.InvariantCulture, out result))
                    result = new System.TimeSpan(0);
                return result;
            }
            set
            {
                Value = value.ToString();
            }
        }

        public static implicit operator JSONNode(System.TimeSpan aTimeSpan)
        {
            return new JSONString(aTimeSpan.ToString());
        }

        public static implicit operator System.TimeSpan(JSONNode aNode)
        {
            return aNode.AsTimeSpan;
        }
        #endregion TimeSpan

        #region Guid
        public virtual System.Guid AsGuid
        {
            get
            {
                System.Guid result;
                System.Guid.TryParse(Value, out result);
                return result;
            }
            set
            {
                Value = value.ToString();
            }
        }

        public static implicit operator JSONNode(System.Guid aGuid)
        {
            return new JSONString(aGuid.ToString());
        }

        public static implicit operator System.Guid(JSONNode aNode)
        {
            return aNode.AsGuid;
        }
        #endregion Guid

        #region ByteArray
        public virtual byte[] AsByteArray
        {
            get
            {
                if (this.IsNull || !this.IsArray)
                    return null;
                int count = Count;
                byte[] result = new byte[count];
                for (int i = 0; i < count; i++)
                    result[i] = this[i].AsByte;
                return result;
            }
            set
            {
                if (!IsArray || value == null)
                    return;
                Clear();
                for (int i = 0; i < value.Length; i++)
                    Add(value[i]);
            }
        }

        public static implicit operator JSONNode(byte[] aByteArray)
        {
            return new JSONArray { AsByteArray = aByteArray };
        }

        public static implicit operator byte[](JSONNode aNode)
        {
            return aNode.AsByteArray;
        }
        #endregion ByteArray
        #region ByteList
        public virtual List<byte> AsByteList
        {
            get
            {
                if (this.IsNull || !this.IsArray)
                    return null;
                int count = Count;
                List<byte> result = new List<byte>(count);
                for (int i = 0; i < count; i++)
                    result.Add(this[i].AsByte);
                return result;
            }
            set
            {
                if (!IsArray || value == null)
                    return;
                Clear();
                for (int i = 0; i < value.Count; i++)
                    Add(value[i]);
            }
        }

        public static implicit operator JSONNode(List<byte> aByteList)
        {
            return new JSONArray { AsByteList = aByteList };
        }

        public static implicit operator List<byte> (JSONNode aNode)
        {
            return aNode.AsByteList;
        }
        #endregion ByteList

        #region StringArray
        public virtual string[] AsStringArray
        {
            get
            {
                if (this.IsNull || !this.IsArray)
                    return null;
                int count = Count;
                string[] result = new string[count];
                for (int i = 0; i < count; i++)
                    result[i] = this[i].Value;
                return result;
            }
            set
            {
                if (!IsArray || value == null)
                    return;
                Clear();
                for (int i = 0; i < value.Length; i++)
                    Add(value[i]);
            }
        }

        public static implicit operator JSONNode(string[] aStringArray)
        {
            return new JSONArray { AsStringArray = aStringArray };
        }

        public static implicit operator string[] (JSONNode aNode)
        {
            return aNode.AsStringArray;
        }
        #endregion StringArray
        #region StringList
        public virtual List<string> AsStringList
        {
            get
            {
                if (this.IsNull || !this.IsArray)
                    return null;
                int count = Count;
                List<string> result = new List<string>(count);
                for (int i = 0; i < count; i++)
                    result.Add(this[i].Value);
                return result;
            }
            set
            {
                if (!IsArray || value == null)
                    return;
                Clear();
                for (int i = 0; i < value.Count; i++)
                    Add(value[i]);
            }
        }

        public static implicit operator JSONNode(List<string> aStringList)
        {
            return new JSONArray { AsStringList = aStringList };
        }

        public static implicit operator List<string> (JSONNode aNode)
        {
            return aNode.AsStringList;
        }
        #endregion StringList

        #region NullableTypes
        public static implicit operator JSONNode(int? aValue)
        {
            if (aValue == null)
                return JSONNull.CreateOrGet();
            return new JSONNumber((int)aValue);
        }
        public static implicit operator int?(JSONNode aNode)
        {
            if (aNode == null || aNode.IsNull)
                return null;
            return aNode.AsInt;
        }

        public static implicit operator JSONNode(float? aValue)
        {
            if (aValue == null)
                return JSONNull.CreateOrGet();
            return new JSONNumber((float)aValue);
        }
        public static implicit operator float? (JSONNode aNode)
        {
            if (aNode == null || aNode.IsNull)
                return null;
            return aNode.AsFloat;
        }

        public static implicit operator JSONNode(double? aValue)
        {
            if (aValue == null)
                return JSONNull.CreateOrGet();
            return new JSONNumber((double)aValue);
        }
        public static implicit operator double? (JSONNode aNode)
        {
            if (aNode == null || aNode.IsNull)
                return null;
            return aNode.AsDouble;
        }

        public static implicit operator JSONNode(bool? aValue)
        {
            if (aValue == null)
                return JSONNull.CreateOrGet();
            return new JSONBool((bool)aValue);
        }
        public static implicit operator bool? (JSONNode aNode)
        {
            if (aNode == null || aNode.IsNull)
                return null;
            return aNode.AsBool;
        }

        public static implicit operator JSONNode(long? aValue)
        {
            if (aValue == null)
                return JSONNull.CreateOrGet();
            return new JSONNumber((long)aValue);
        }
        public static implicit operator long? (JSONNode aNode)
        {
            if (aNode == null || aNode.IsNull)
                return null;
            return aNode.AsLong;
        }

        public static implicit operator JSONNode(short? aValue)
        {
            if (aValue == null)
                return JSONNull.CreateOrGet();
            return new JSONNumber((short)aValue);
        }
        public static implicit operator short? (JSONNode aNode)
        {
            if (aNode == null || aNode.IsNull)
                return null;
            return aNode.AsShort;
        }
        #endregion NullableTypes
    }
}
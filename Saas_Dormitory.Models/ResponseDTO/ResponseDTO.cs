using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saas_Dormitory.Models.ResponseDTO
{
    public class ResponseDTO
    {
        int _id;
        int _count;
        long _idl;
        bool _valid;
        string _message;
        string _roleName;
        string _roleID;
        string _userId;
        decimal _TotatAmount;
        string _url;
        int _cartCount;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public int CartCount
        {
            get { return _cartCount; }
            set { _cartCount = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public long IdL
        {
            get { return _idl; }
            set { _idl = value; }
        }

        public string Msg
        {
            get { return _message; }
            set { _message = value; }
        }

        public string RoleName
        {
            get { return _roleName; }
            set { _roleName = value; }
        }

        public string RoleID
        {
            get { return _roleID; }
            set { _roleID = value; }
        }
        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public bool Valid
        {
            get { return _valid; }
            set { _valid = value; }
        }

        public decimal TotatAmount
        {
            get { return _TotatAmount; }
            set { _TotatAmount = value; }
        }


        public string URL
        {
            get { return _url; }
            set { _url = value; }
        }
        public ResponseDTO(string defaultMessage)
        {
            Msg = defaultMessage;
            Valid = false;
        }

        /// <summary>
        /// Initializes the Success variable to 'false.'
        /// </summary>
        public ResponseDTO()
        {
            Valid = false;
        }

    }

    public class ListResponseDTO<DtoType> : ResponseDTO
    {
        public ListResponseDTO()
            : base()
        {
            Items = new List<DtoType>();
        }

        public ListResponseDTO(string defaultMessage)
            : base(defaultMessage)
        {
            Items = new List<DtoType>();
        }
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public List<DtoType> Items;
    }

    public class SingleItemResponseDTO<DtoType> : ResponseDTO where DtoType : new()
    {
        public SingleItemResponseDTO()
            : base()
        {
            Item = new DtoType();
        }

        public SingleItemResponseDTO(string defaultMessage)
            : base(defaultMessage)
        {
            Item = new DtoType();
        }
        public DtoType Item;
    }
}

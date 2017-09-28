using System;
using System.Collections.Generic;
using System.Web;

namespace Enferno.Web.StormUtils {
    [Obsolete("Will be removed in future version.")]
	public abstract class Query {
		public const string ParamCategoryId = "cat";
		public const string ParamParametric = "par";
		public const string ParamCustomerId = "cus";
		public const string ParamDivisionId = "div";
		public const string ParamProductId = "prd";
		public const string ParamPartNo = "pno";
		public const string ParamPriceListId = "prl";
		public const string ParamQuantity = "qty";
		public const string ParamManufacturerId = "mfr";
		public const string ParamFlagId = "flg";
		public const string ParamStatus = "sts";
		public const string ParamType = "typ";
		public const string ParamSearch = "src";
		public const string ParamBasketId = "bas";
		public const string ParamOrderId = "ord";
		public const string ParamFamilyId = "fam";
		public const string ParamAddressId = "adr";
		public const string ParamFilterCategory = "catf";
		public const string ParamFilterManufacturer = "mfrf";
		public const string ParamFilterFlag = "flgf";
		public const string ParamFilterParametric = "parf";
		public const string ParamFilterOnHand = "ohf";
		public const string ParamFilterPriceMin = "pmin";
		public const string ParamFilterPriceMax = "pmax";
		public const string ParamPagingIndex = "ind";
		public const string ParamSortOrder = "srt";
		public const string ParamError = "err";
		public const string ParamAction = "act";

		public static int? CategoryId {
			get {
				return GetQueryInt(ParamCategoryId);
			}
		}

		public static string Parametric {
			get {
				return GetQueryString(ParamParametric);
			}
		}

		public static int? CustomerId {
			get {
				return GetQueryInt(ParamCustomerId);
			}
		}

		public static int? DivisionId {
			get {
				return GetQueryInt(ParamDivisionId);
			}
		}

		public static int? ProductId {
			get {
				return GetQueryInt(ParamProductId);
			}
		}

		public static string PartNo {
			get {
				return GetQueryString(ParamPartNo);
			}
		}

		public static int? PriceListId {
			get {
				return GetQueryInt(ParamPriceListId);
			}
		}

		public static int? Quantity {
			get {
				return GetQueryInt(ParamQuantity);
			}
		}

		public static int? ManufacturerId {
			get {
				return GetQueryInt(ParamManufacturerId);
			}
		}

		public static int? FlagId {
			get {
				return GetQueryInt(ParamFlagId);
			}
		}

		public static string StatusIdSeed {
			get {
				string seed = GetQueryString(ParamStatus);
				if(seed != null)
					seed = seed.Replace("_", ",");
				return seed;
			}
		}

		public static string TypeIdSeed {
			get {
				string seed = GetQueryString(ParamType);
				if(seed != null)
					seed = seed.Replace("_", ",");
				return seed;
			}
		}

		public static string Search {
			get {
				string s = GetQueryString(ParamSearch);
				if(s != null && s.Contains("=")) {
					s = s.Replace("=", " ");
				}
				return s;
			}
		}

		public static int? BasketId {
			get {
				return GetQueryInt(ParamBasketId);
			}
		}

		public static int? OrderId {
			get {
				return GetQueryInt(ParamOrderId);
			}
		}
		public static int? FamilyId {
			get {
				return GetQueryInt(ParamFamilyId);
			}
		}
		public static int? AddressId {
			get {
				return GetQueryInt(ParamAddressId);
			}
		}
		public static string Error {
			get {
				return GetQueryString(ParamError);
			}
		}
		public static string Action {
			get {
				return GetQueryString(ParamAction);
			}
		}
		public static string FilterCategory {
			get {
				return GetQueryString(ParamFilterCategory);
			}
		}
		public static string FilterManufacturer {
			get {
				return GetQueryString(ParamFilterManufacturer);
			}
		}
		public static string FilterFlag {
			get {
				return GetQueryString(ParamFilterFlag);
			}
		}
		public static string FilterParametric {
			get {
				return GetQueryString(ParamFilterParametric);
			}
		}
		public static decimal? FilterPriceMin {
            get
            {
                return GetQueryDecimal(ParamFilterPriceMin);
			}
		}
		public static decimal? FilterPriceMax {
            get
            {
                return GetQueryDecimal(ParamFilterPriceMax);
			}
		}
		public static string FilterOnHand {
			get {
				return GetQueryString(ParamFilterOnHand);
			}
		}
		public static int? PagingIndex {
			get {
				return GetQueryInt(ParamPagingIndex);
			}
		}
		public static string SortOrder {
			get {
				return GetQueryString(ParamSortOrder);
			}
		}

		public static int? GetQueryInt(string param) {
			int? _return = null;
			string query = HttpContext.Current.Request.QueryString[param];
			if(query != null) {
				int parse;
				if(int.TryParse(query, out parse))
					_return = parse;
			}
			return _return;
		}

        public static decimal? GetQueryDecimal(string param)
        {
            decimal? _return = null;
            string query = HttpContext.Current.Request.QueryString[param];
            if (query != null)
            {
                decimal parse;
                if (decimal.TryParse(query, out parse))
                    _return = parse;
            }
            return _return;
        }

		public static bool? GetQueryBool(string param, bool? defaultNoValueAs) {
			bool? _return = null;
			string query = HttpContext.Current.Request.QueryString[param];
			if(query != null) {
				if(query == "null") {
					return null;
				}
				bool parse;
				if(bool.TryParse(query, out parse))
					return parse;
				if(query == "1")
					return true;
				if(query == "0")
					return false;
			} else {
				_return = defaultNoValueAs;
			}
			return _return;
		}

		public static string GetQueryString(string param) {
			return HttpContext.Current.Request.QueryString[param];
		}
	}
}

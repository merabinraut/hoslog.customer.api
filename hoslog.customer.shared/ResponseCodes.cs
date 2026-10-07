namespace hoslog.customer.shared
{
    public static class ResponseCodes
    {
        public const string Code200_Success = "200";
        public const string Code201_Created = "201";
        public const string Code202_Accepted = "202";

        public const string Code400_BadRequest = "400";
        public const string Code401_Unauthorized = "401";
        public const string Code402_PaymentRequired = "402";
        public const string Code403_Forbidden = "403";
        public const string Code404_ResourceNotFound = "404";
        public const string Code405_MethodNotAllowed = "405";
        public const string Code406_NotAcceptable = "406";
        public const string Code407_ProxyAuthenticationRequired = "407";
        public const string Code408_RequestTimeout = "408";
        public const string Code409_Conflict = "409";
        public const string Code410_Gone = "410";
        public const string Code415_UnsupportedMediaType = "415";
        public const string Code422_UnprocessableEntity = "422";

        public const string Code500_InternalServerError = "500";
        public const string Code501_NotImplemented = "501";
        public const string Code502_BadGateway = "502";
        public const string Code503_ServiceUnavailable = "503";
        public const string Code504_GatewayTimeout = "504";
        public const string CodeC701_RecordFetched = "C701";
        public const string CodeC702_Recordnotfound = "C702";
        //Common Code
        public const string CodeCUE100_Invalid_request = "CUE100";
        public const string CodeCUE101_Invalid_club = "CUE101";
        public const string CodeCUE102_Invalid_host = "CUE102";
        public const string CodeCUE102_Invalid_agent_type = "CUE107";
        public const string CodeCUE105_User_not_found = "CUE105";
        public const string CodeCUE106_Club_Id_is_required = "CUE106";
        public const string CodeCUE109_Invalid_date_format = "CUE109";
        public const string CodeCUE110_Host_id_or_host_code_is_required = "CUE110";
        public const string CodeCUE111_Club_not_found = "CUE111";
        public const string CodeCUE112_User_Id_is_required = "CUE112";
        public const string CodeCUE103_Invalid_customer = "CUE103";
        public const string CodeCUE113_Invalid_transaction_mode = "CUE113";
        public const string CodeCUE114_New_Password_and_confirm_password_does_not_match = "CUE114";
        public const string CodeCUEREG110_Invalid_current_password = "CUEREG110";
        public const string CodeCUEREG111_New_password_cannot_be_same_as_current_password = "CUEREG111";
        public const string CodeCUE115_Login_id_is_required = "CUE115";
        public const string CodeCUE116_Password_must_be_between_8_to_32_characters = "CUE116";
        public const string CodeCUE117_Profile_image_is_required = "CUE117";
        public const string CodeCUE118_Profile_image_must_be_less_than_5MB = "CUE118";
        public const string CodeCUE125_Something_Went_Wrong_Contact_Admin = "CUE125";



        public const string CodeCUE104_ClubId_or_Clubcode_is_required = "CUE104";
        public const string CodeCUSBM104_Bookmark_added_successfully = "CUSBM104";
        public const string CodeCUSBM105_Bookmark_removed_successfully = "CUSBM105";
        public const string CodeCUSBM106_Bookmark_created_successfully = "CUSBM106";
        public const string CodeCUEHS100_Date_must_be_in_the_future = "CUEHS100";
        public const string CodeCUSNTF100_Notification_read_status_updated_successfully = "CUSNTF100";
        public const string CodeCUENTF101_Invalid_notification_details = "CUENTF101";
        public const string CodeCUSIN100_Your_inquiry_has_been_sent = "CUSIN100";
        public const string CodeCUSPL100_Plan_details_fetched_successfully = "CUSPL100";
        public const string CodeCUEPF100_This_email_address_is_already_in_use = "CUEPF100";
        public const string CodeCUSPF101_Profile_updated = "CUSPF101";
        public const string CodeCUEPF102_Failed_to_generate_URL = "CUEPF102";
        public const string CodeCUEPF103_Customer_deleted_successfully = "CUEPF103";
        public const string CodeCUEPF104_Failed_to_generate_URL = "CUEPF104";
        public const string CodeCUSPF105_File_uploaded_successfully = "CUSPF105";
        public const string CodeCUEAUT100_Username_is_required = "CUEAUT100";
        public const string CodeCUEAUT101_Refresh_token_is_required = "CUEAUT101";
        public const string CodeCUEREG116_Email_Address_is_required = "CUEREG116";
        public const string CodeCUEREG117_DOB_is_required = "CUEREG117";
        public const string CodeCUEREG118_Invalid_Email_Format = "CUEREG118";


        //Conversion Error Codes 
        public const string CodeCUECM100_Invalid_source_page = "CUECM100";
        public const string CodeCUECM101_HostId_required_for_host_detail_journey = "CUECM101";
        public const string CodeCUECM102_Invalid_event_name = "CUECM102";
        public const string CodeCUECM103_The_reservation_belongs_to_another_customer= "CUECM103";
        public const string CodeCUECM104_Invalid_journey_Id = "CUECM104";

        // Custom Error codes
        public const string CodeE601_InvalidSignatureToken = "E601";
        public const string CodeE602_InvalidProcessId = "E602";
        public const string CodeE603_InvalidEncryption = "E603";

        //Common messages
        public const string CodeC701_RecordFetchedSuccess = "C701";
        public const string CodeC702_RecordNotFound = "C702";
        public const string CodeC703_InvalidDateFormat = "C703";
        public const string CodeA1005_FailedToChangePassword = "A1005";
        public const string CodeR1015_NotMatchPassword = "C1015";
        public const string CodeR1020_NewPasswordNotSameAsCurrent = "R1020";
        public const string CodeC1036_InvalidCurrentPassword = "C1036";
        public const string CodeA1040_InvalidCategory = "A1040";
        public const string CodeC704_OneOrMoreField = "C704";
        public const string CodeC705_InvalidBusinesType = "C705";
        public const string CodeC706_ConfirmationShouldChecked = "C706";
        public const string CodeC707_InvalidReferalType = "C707";
        public const string CodeC708_AccountAlreadyExists = "C708";
        public const string CodeC709_SpecialCharacterNotAllowed = "C709";
        public const string CodeC710_ValidationFailed = "C710";
        public const string CUE101_InvalidClub = "CUE101";
        public const string CUE100_InvalidRequest = "CUE100";
        public const string CE465_CustomerIdRequired = "CE465";
        public const string CE466_PasswordRequired = "CE466";
        public const string CE467_ConfirmPasswordRequired = "CE467";
        public const string CE463_ProcessIdRequired = "CE463";
        public const string CE468_NicknameRequired = "CE468";
        public const string CE469_MobileNumberRequired = "CE469";
        public const string CE470_OTPCodeRequired = "CE470";
        public const string CE464_PasswordAndConfirmPasswordDoesnotMatch = "CE464";
        public const string CE474_ActionIpRequired = "CE474";
        public const string CE475_ActionPlatformRequired = "CE475";
        public const string CUE106_ClubIdRequired = "CUE106";
        public const string CE476_InvalidAuthorizationHeader = "CE476";
        public const string CE477_Unauthorized = "CE477";
        public const string CE478_InvalidToken = "CE478";
        public const string CUE124_DateTimeRequired = "CUE124";

        //enquery 
        public const string CUEEN100_enqueryTypeIsRequired = "CUEEN100";
        public const string CUEEN101_descriptionIsRequired = "CUEEN101";
        public const string CodeCUEINQ100_Invalid_Inquery_type = "CUEINQ100";
        public const string CodeCUEINQ101_company_name_is_required = "CUEINQ101";



        //DB Message
        #region  Review
        public const string CUSREV101_ReviewRemarkListFetchedSuccess = "CUSREV101";
        public const string CUSREV102_DichotomousQuestionsAndAnswerListFetchedSuccess = "CUSREV102";
        #endregion
        #region Reservation
        public const string CUSRES100_ReservationDetailsFetchedSuccess = "CUSRES100";
        public const string CUSRES101_ReservationListFetchedSuccess = "CUSRES101";
        public const string CUSRES104_ReservationDetailsWithTimeFetchedSuccess = "CUSRES104";
        public const string CUERES109_ReservationPlansNotAvailable = "CUERES109";
        public const string CUERES122_MVPHostIdRequired = "CUERES122";
        public const string CUERES123_HostListRequired = "CUERES123";
        public const string CUERES124_RatingScaleRequirement = "CUERES124";
        public const string CUERES125_ReviewIdListIsRequired = "CUERES125";
        public const string CUERES126_DichotomousquestionAndAnswerIdListRequired = "CUERES126";
        public const string CUERES127_ReservationIdRequired = "CUERES127";
        public const string CUERES128_ReservationTimeRequired = "CUERES128";
        public const string CUERES131_ReservationDateRequired = "CUERES131";
        public const string CUERES129_NumberOfPeopleRequired = "CUERES129";
        public const string CUERES133_PaymentTypeRequired = "CUERES133";
        public const string CUERES132_PlanIdRequired = "CUERES132";
        public const string CUERES114_HostIdRequired = "CUERES114";
        public const string CUERES138_ReservationHoldIdRequired = "CUERES138";
        public const string CUERES139_ReservationTraceIdRequired = "CUERES139";
        public const string CUERES134_ReservationInvalidOTP = "CUERES134";
        #endregion
        #region Registration
        public const string CUEREG100_RegistrationMustAgreeTermsAndCondition = "CUEREG100";
        public const string CUEREG101_DuplicateMobileNumberOrNickname = "CUEREG101";
        public const string CUEREG105_NicknameIsRequired = "CUEREG105";
        public const string CUEREG106_MobileNumberIsRequired = "CUEREG106";
        public const string CUEREG108_HoldCustomerIdRequired = "CUEREG108";
        public const string CUEREG109_NewPasswordAndConfirmPasswordNotMatched = "CUEREG109";
        public const string CUEREG111_NewPasswordCannotBeSameAsCurrentPassword = "CUEREG111";
        public const string CUEREG110_InvalidCurrentPassword = "CUEREG110";
        public const string CUEREG114_OldPasswordRequired = "CUEREG114";
        public const string CUEREG115_NewPasswordRequired = "CUEREG115";
        #endregion
        #region Recommendation
        public const string CUEREC100_RecommendationHostListFetchedSuccessfully = "CUEREC100";
        public const string CUEREC101_MainpageRecommendedClubAndHostListFetchedSuccessfully = "CUEREC101";
        public const string CUEREC102_RecommendationClubListFetchedSuccessfully = "CUEREC102";
        public const string CUEREC103_HomepageRecommendedClubAndHostListFetchedSuccessfully = "CUEREC103";
        #endregion

        #region Profile
        public const string CUE120_InvalidDateFormat = "CUE120";
        public const string CUE121_FutureDOB = "CUE121";
        #endregion

        #region refer
        public const string CUEREF100_ReferCodeIsRequired = "CUEREF100";
        #endregion

        public static string GetMessageForResponseCode(string code) => code switch
        {
            Code200_Success => ResponseMessages.Msg200_Success,
            Code201_Created => ResponseMessages.Msg201_Created,
            Code202_Accepted => ResponseMessages.Msg202_Accepted,

            Code400_BadRequest => ResponseMessages.Msg400_BadRequest,
            Code401_Unauthorized => ResponseMessages.Msg401_Unauthorized,
            Code402_PaymentRequired => ResponseMessages.Msg402_PaymentRequired,
            Code403_Forbidden => ResponseMessages.Msg403_Forbidden,
            Code404_ResourceNotFound => ResponseMessages.Msg404_ResourceNotFound,
            Code405_MethodNotAllowed => ResponseMessages.Msg405_MethodNotAllowed,
            Code406_NotAcceptable => ResponseMessages.Msg406_NotAcceptable,
            Code407_ProxyAuthenticationRequired => ResponseMessages.Msg407_ProxyAuthenticationRequired,
            Code408_RequestTimeout => ResponseMessages.Msg408_RequestTimeout,
            Code409_Conflict => ResponseMessages.Msg409_Conflict,
            Code410_Gone => ResponseMessages.Msg410_Gone,
            Code415_UnsupportedMediaType => ResponseMessages.Msg415_UnsupportedMediaType,
            Code422_UnprocessableEntity => ResponseMessages.Msg422_UnprocessableEntity,

            Code500_InternalServerError => ResponseMessages.Msg500_InternalServerError,
            Code501_NotImplemented => ResponseMessages.Msg501_NotImplemented,
            Code502_BadGateway => ResponseMessages.Msg502_BadGateway,
            Code503_ServiceUnavailable => ResponseMessages.Msg503_ServiceUnavailable,
            Code504_GatewayTimeout => ResponseMessages.Msg504_GatewayTimeout,

            CodeE601_InvalidSignatureToken => ResponseMessages.MsgE601_InvalidSignatureToken,
            CodeE602_InvalidProcessId => ResponseMessages.MsgE601_InvalidSignatureToken,
            _ => string.Empty
        };
    }

    public static class ResponseMessages
    {
        public const string Msg200_Success = "Success";
        public const string Msg201_Created = "Created";
        public const string Msg202_Accepted = "Accepted";

        public const string Msg400_BadRequest = "Bad Request";
        public const string Msg401_Unauthorized = "Unauthorized, you are not authorized to access the resource";
        public const string Msg402_PaymentRequired = "Payment Required";
        public const string Msg403_Forbidden = "Forbidden, you don't have permission to access this resource";
        public const string Msg404_ResourceNotFound = "Resource not found";
        public const string Msg405_MethodNotAllowed = "Method Not Allowed";
        public const string Msg406_NotAcceptable = "Not Acceptable";
        public const string Msg407_ProxyAuthenticationRequired = "Proxy Authentication Required (RFC 7235)";
        public const string Msg408_RequestTimeout = "Request Timeout";
        public const string Msg409_Conflict = "A conflict request, resource already exists";
        public const string Msg410_Gone = "Gone, the resource requested is no longer available";
        public const string Msg415_UnsupportedMediaType = "Unsupported Media Type";
        public const string Msg422_UnprocessableEntity = "Unprocessable Entity";
        public const string Msg429_TooManyRequests = "Too Many Requests";

        public const string Msg500_InternalServerError = "Internal Server Error, an error occurred while processing your request";
        public const string Msg501_NotImplemented = "Not Implemented";
        public const string Msg502_BadGateway = "Bad Gateway";
        public const string Msg503_ServiceUnavailable = "Service Unavailable";
        public const string Msg504_GatewayTimeout = "Gateway Timeout";

        public const string MsgE601_InvalidSignatureToken = "Invalid signature token";
        public const string MsgE602_InvalidProcessId = "Process ID is invalid or expired";
        public const string MsgE603_InvalidEncryption = "The encrypted data is invalid";
    }
}

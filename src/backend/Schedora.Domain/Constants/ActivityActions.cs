namespace SocialScheduler.Domain.Constants
{
    public static class ActivityActions
    {
        // Authentication
        public const string USER_LOGIN = "user_login";
        public const string USER_LOGOUT = "user_logout";
        public const string USER_REGISTER = "user_register";
        public const string PASSWORD_RESET_REQUESTED = "password_reset_requested";
        public const string PASSWORD_RESET_COMPLETED = "password_reset_completed";
        public const string EMAIL_VERIFIED = "email_verified";
        
        // User Management
        public const string PROFILE_UPDATED = "profile_updated";
        public const string SUBSCRIPTION_UPGRADED = "subscription_upgraded";
        public const string SUBSCRIPTION_DOWNGRADED = "subscription_downgraded";
        public const string ACCOUNT_DEACTIVATED = "account_deactivated";
        
        // Social Accounts
        public const string SOCIAL_ACCOUNT_CONNECTED = "social_account_connected";
        public const string SOCIAL_ACCOUNT_DISCONNECTED = "social_account_disconnected";
        public const string SOCIAL_ACCOUNT_TOKEN_REFRESHED = "social_account_token_refreshed";
        public const string SOCIAL_ACCOUNT_TOKEN_EXPIRED = "social_account_token_expired";
        
        // Posts
        public const string POST_CREATED = "post_created";
        public const string POST_UPDATED = "post_updated";
        public const string POST_DELETED = "post_deleted";
        public const string POST_SCHEDULED = "post_scheduled";
        public const string POST_RESCHEDULED = "post_rescheduled";
        public const string POST_PUBLISHED = "post_published";
        public const string POST_FAILED = "post_failed";
        public const string POST_CANCELLED = "post_cancelled";
        public const string POST_DUPLICATED = "post_duplicated";
        
        // Approval Workflow
        public const string POST_SUBMITTED_FOR_APPROVAL = "post_submitted_for_approval";
        public const string POST_APPROVED = "post_approved";
        public const string POST_REJECTED = "post_rejected";
        
        // Media
        public const string MEDIA_UPLOADED = "media_uploaded";
        public const string MEDIA_DELETED = "media_deleted";
        public const string FOLDER_CREATED = "folder_created";
        
        // Templates & Queues
        public const string TEMPLATE_CREATED = "template_created";
        public const string TEMPLATE_USED = "template_used";
        public const string QUEUE_CREATED = "queue_created";
        public const string QUEUE_UPDATED = "queue_updated";
        
        // Team
        public const string TEAM_MEMBER_INVITED = "team_member_invited";
        public const string TEAM_MEMBER_REMOVED = "team_member_removed";
        public const string TEAM_MEMBER_ROLE_CHANGED = "team_member_role_changed";
        
        // Security
        public const string SUSPICIOUS_LOGIN_ATTEMPT = "suspicious_login_attempt";
        public const string API_KEY_CREATED = "api_key_created";
        public const string API_KEY_REVOKED = "api_key_revoked";
    }
}
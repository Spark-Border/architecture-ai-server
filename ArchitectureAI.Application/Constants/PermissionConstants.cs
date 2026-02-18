namespace ArchitectureAI.Application.Constants
{
    public static class Permissions
    {
        public static class Users
        {
            public const string View = "users.view";
            public const string Create = "users.create";
            public const string Edit = "users.edit";
            public const string Delete = "users.delete";
        }

        public static class Roles
        {
            public const string View = "roles.view";
            public const string Create = "roles.create";
            public const string Edit = "roles.edit";
            public const string Delete = "roles.delete";
        }

        // Example Business Permissions
        public static class Projects
        {
            public const string View = "projects.view";
            public const string Create = "projects.create";
            public const string Edit = "projects.edit";
            public const string Delete = "projects.delete";
        }

        public static List<string> All =>
            [
                Users.View,
                Users.Create,
                Users.Edit,
                Users.Delete,
                Roles.View,
                Roles.Create,
                Roles.Edit,
                Roles.Delete,
                Projects.View,
                Projects.Create,
                Projects.Edit,
                Projects.Delete,
            ];
    }
}

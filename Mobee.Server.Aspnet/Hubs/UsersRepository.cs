namespace Mobee.Server.Aspnet.Hubs
{
    public class UsersRepository
    {
        private class User
        {
            public string ConnectionId { get; set; }
            
            public string? UserName { get; set; }

            public string? GroupName { get; set; }
        }

        private List<User> _users = new List<User>();

        public void UserConnected(string connectionId, string? userName, string? groupName)
        {
            var find = _users.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (find != null)
            {
                find.UserName = userName;
                find.GroupName = groupName;
            }
            else
            {
                _users.Add(new User()
                {
                    ConnectionId = connectionId,
                    UserName = userName,
                    GroupName = groupName
                });
            }
        }

        public void UserDisconnected(string connectionId)
        {
            var find = _users.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (find != null)
            {
                _users.Remove(find);
            }
        }

        public List<string> GetGroupUsersExcept(string groupName, string userName)
        {
            return _users
                .Where(x => x.GroupName == groupName && x.UserName != userName)
                .Select(x => x.UserName)
                .ToList();
        }

        public string? GetUserGroup(string connectionId)
        {
            var find = _users.FirstOrDefault(x => x.ConnectionId == connectionId);
            
            return find?.GroupName;
        }
    }
}

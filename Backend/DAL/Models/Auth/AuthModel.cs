namespace DAL;

public class AuthModel
{
	public string[] Errors { get; set; }
	public bool HasErrors
	{
		get => Errors != null && Errors.Length > 0;
		set => throw new NotImplementedException();
	}

	public bool IsAuthenticated { get; set; }
	public string Username { get; set; }
	public string Email { get; set; }
	public List<string> Roles { get; set; }
	public string Token { get; set; }
	public DateTime ExpiresOn { get; set; }
}
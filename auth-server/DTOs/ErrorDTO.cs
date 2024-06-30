namespace auth_server.DTOs;

public class ErrorDTO
{
    public ErrorDescriptionDTO error { get; set; }

    public ErrorDTO(int code, string message = "No description provided")
    {
        error = new(code, message);
    }
}

public class ErrorDescriptionDTO
{
    public int code { get; set; }
    public string message { get; set; }

    public ErrorDescriptionDTO(int code, string message)
    {
        this.code = code;
        this.message = message;
    }
}
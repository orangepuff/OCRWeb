namespace OCRWeb.ProjectManagement.Api.Endpoints.UpdateProject;

public class UpdateProjectRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

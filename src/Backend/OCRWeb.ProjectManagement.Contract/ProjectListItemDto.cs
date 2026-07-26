namespace OCRWeb.ProjectManagement.Contract;

/// <summary>Lightweight list item for "my projects".</summary>
public record ProjectListItemDto(int Id, string Name, DateTime InsertedTime);

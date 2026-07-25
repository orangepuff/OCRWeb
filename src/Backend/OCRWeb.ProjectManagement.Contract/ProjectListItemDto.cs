namespace OCRWeb.ProjectManagement.Contract;

/// <summary>Lightweight list item for "my projects".</summary>
public record ProjectListItemDto(Guid Id, string Name, DateTime InsertedTime);

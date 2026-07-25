export const messages = {
  project: {
    nameRequired: 'Project name is required',
    pdfRequired: 'Please select a PDF file to upload',
    createSuccess: 'Project created successfully',
    createError: 'Failed to create project',
    uploadError: 'Project was created, but the PDF failed to upload',
    empty: 'You have no projects yet.',
    updateSuccess: 'Project updated',
    updateError: 'Failed to rename project',
    deleteSuccess: 'Project deleted',
    deleteError: 'Failed to delete project',
    fileUploadSuccess: 'File uploaded',
    fileUploadError: 'Failed to upload file',
    fileDeleteSuccess: 'File deleted',
    fileDeleteError: 'Failed to delete file',
    loadError: 'Failed to load project',
    cropLoadError: 'Failed to load the PDF for cropping',
    noFileError: 'No PDF file found for this project',
    selectionRequired: 'Drag on the preview to select a crop area',
    cropError: 'Failed to crop the PDF',
    cropSuccess: 'PDF cropped successfully'
  }
} as const;

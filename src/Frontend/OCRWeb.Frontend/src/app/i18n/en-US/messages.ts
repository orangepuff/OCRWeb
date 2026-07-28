export const messages = {
  split: {
    saveSuccess: 'Splits saved successfully',
    saveError: 'Failed to save splits',
    loadError: 'Failed to load the PDF for splitting',
    noFileError: 'No PDF file found for this project',
    resetConfirmTitle: 'Reset splits?',
    resetConfirmMessage: 'This will discard all changes and restore the original bookmark splits.',
    nameRequired: 'Split name is required',
    noBookmarksWarning: 'No bookmarks found in this PDF. Add splits manually.'
  },
  project: {
    nameRequired: 'Project name is required',
    pdfRequired: 'Please select a PDF file to upload',
    createSuccess: 'Project created successfully',
    createError: 'Failed to create project',
    uploadError: 'Project was created, but the PDF failed to upload',
    uploadErrorOnUpdate: 'Project was updated, but the PDF failed to upload',
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
    cropSuccess: 'PDF cropped successfully',
    splitNotImplemented: 'Splitting into sections is not implemented yet.'
  }
} as const;

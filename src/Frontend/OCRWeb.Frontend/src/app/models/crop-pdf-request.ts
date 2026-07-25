export interface CropPdfRequest {
  pageNo: number;
  cropX: number;
  cropY: number;
  width: number;
  height: number;
  fileName?: string;
}

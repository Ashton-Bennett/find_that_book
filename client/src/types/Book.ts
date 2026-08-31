export interface Book {
  title: string;
  authors: string[];
  subjects: string[];
  places: string[];
  people: string[];
  publishers: string[];
  languages: string[];
  firstPublishYear: number | null;
  openLibraryKey: string | null;
  coverUrl: string | null;
  confidenceScore: number;
  explanation: string;
}

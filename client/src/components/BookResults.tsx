import type { Book } from "../types/Book";
import { BookResult } from "./BookResult";

interface BookResultsProps {
  error: string | null;
  loading: boolean;
  results: Book[];
}

export function BookResults({ error, loading, results }: BookResultsProps) {
  return (
    <section className="center">
      {error && <p>{error}</p>}

      {!loading && !error && results.length === 0 && <p>No books found.</p>}

      {results.map((book) => (
        <BookResult key={book.openLibraryKey} book={book} />
      ))}
    </section>
  );
}

import type { Book } from "../types/Book";

interface BookResultProps {
  book: Book;
}

export function BookResult({ book }: BookResultProps) {
  return (
    <div className="book-result">
      <div className="book-cover">
        {book.coverUrl ? (
          <img src={book.coverUrl} alt={`Cover of ${book.title}`} />
        ) : (
          <div className="no-cover">No Cover</div>
        )}
      </div>

      <div className="book-details">
        <h3>
          {book.openLibraryKey ? (
            <a
              href={`https://openlibrary.org${book.openLibraryKey}`}
              target="_blank"
              rel="noopener noreferrer"
              className="purple-title"
            >
              {book.title}
            </a>
          ) : (
            book.title
          )}
        </h3>

        {book.authors?.length > 0 && (
          <p>
            <strong>Author:</strong> {book.authors.join(", ")}
          </p>
        )}

        {book.firstPublishYear && (
          <p>
            <strong>Published:</strong> {book.firstPublishYear}
          </p>
        )}

        {book.subjects?.length > 0 && (
          <p>
            <strong>Subjects:</strong> {book.subjects.join(", ")}
          </p>
        )}

        {book.places?.length > 0 && (
          <p>
            <strong>Places:</strong> {book.places.join(", ")}
          </p>
        )}

        {book.people?.length > 0 && (
          <p>
            <strong>People:</strong> {book.people.join(", ")}
          </p>
        )}

        {book.publishers?.length > 0 && (
          <p>
            <strong>Publisher:</strong> {book.publishers.join(", ")}
          </p>
        )}

        {book.languages?.length > 0 && (
          <p>
            <strong>Languages:</strong> {book.languages.join(", ")}
          </p>
        )}

        {typeof book.confidenceScore === "number" && (
          <p>
            <strong>Match:</strong> {(book.confidenceScore * 100).toFixed(0)}%
          </p>
        )}

        {book.explanation && (
          <p className="match-explanation">{book.explanation}</p>
        )}
      </div>
    </div>
  );
}

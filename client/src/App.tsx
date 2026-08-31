import { useState } from "react";
import heroImg from "./assets/hero.png";
import "./App.css";
import type { Book } from "./types/Book";

function App() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<Book[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!query.trim()) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await fetch(
        `http://localhost:5119/api/books/search?query=${encodeURIComponent(query)}`,
      );

      if (!response.ok) {
        throw new Error("Failed to search for books");
      }

      const data: Book[] = await response.json();

      setResults(data);
    } catch (err) {
      setError(err.message);
      setResults([]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ margin: "4rem" }}>
      <form className="center" onSubmit={handleSubmit}>
        <div className="hero">
          <img src={heroImg} className="base" width="170" height="179" alt="" />
        </div>
        <div>
          <h1>Find That Book</h1>
          <p>
            Enter any combination of <span>title</span>, <span>author</span>, or
            <span>keywords</span>.
          </p>
        </div>
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: "8px",
            marginTop: "16px",
          }}
        >
          <input
            className="counter"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          ></input>
          <button type="submit" className="counter" disabled={loading}>
            {loading ? "Searching..." : "Search"}
          </button>
        </div>
      </form>
      <section className="center">
        {error && <p>{error}</p>}

        {!loading && !error && results.length === 0 && <p>No books found.</p>}

        {results.map((book) => (
          <div key={book.openLibraryKey} className="book-result">
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
                  <strong>Match:</strong>{" "}
                  {(book.confidenceScore * 100).toFixed(0)}%
                </p>
              )}

              {book.explanation && (
                <p className="match-explanation">{book.explanation}</p>
              )}
            </div>
          </div>
        ))}
      </section>
    </div>
  );
}

export default App;

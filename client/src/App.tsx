import { useState, type FormEvent } from "react";
import "./App.css";
import { BookResults } from "./components/BookResults";
import { SearchForm } from "./components/SearchForm";
import type { Book } from "./types/Book";

function App() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<Book[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
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
      setError(err instanceof Error ? err.message : "Failed to search for books");
      setResults([]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ margin: "4rem" }}>
      <SearchForm
        query={query}
        loading={loading}
        onQueryChange={setQuery}
        onSubmit={handleSubmit}
      />
      <BookResults error={error} loading={loading} results={results} />
    </div>
  );
}

export default App;

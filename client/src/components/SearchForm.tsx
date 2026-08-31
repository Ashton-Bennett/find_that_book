import type { FormEventHandler } from "react";
import heroImg from "../assets/hero.png";

interface SearchFormProps {
  query: string;
  loading: boolean;
  onQueryChange: (query: string) => void;
  onSubmit: FormEventHandler<HTMLFormElement>;
}

export function SearchForm({
  query,
  loading,
  onQueryChange,
  onSubmit,
}: SearchFormProps) {
  return (
    <form
      className="center"
      onSubmit={onSubmit}
      style={{ marginBottom: "4rem" }}
    >
      <div className="hero">
        <img
          src={heroImg}
          className="base"
          width="170"
          height="179"
          alt="Find That Book"
        />
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
          onChange={(event) => onQueryChange(event.target.value)}
        />
        <button type="submit" className="counter" disabled={loading}>
          {loading ? "Searching..." : "Search"}
        </button>
      </div>
    </form>
  );
}

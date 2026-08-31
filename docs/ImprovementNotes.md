## Core expectations:

    1. Support sparse input such as only a title or only an author (for example: "dickens" or "tale two cities").
    Tale of two cites search shows:
    Match: 0%
    No strong matches found for the requested criteria.
    Despite results showing expected books.
    The matching functionality could be better.

    2. Account for Open Library data quality issues rather than assuming every author_name value is a primary
    author.
    Currently searches show multiple author names. Need to check to ensure authors list are actual authors not some other contributor.

    3. Make loading states more visually appealing in the UI. Biggest area is while searching. The search takes a long time so show something to entertain the user.

## Future Enhancements:

    4. Build CI/CD pipeline that will run tests before allowing PRs to complete.

    5. Deploy the app to a public website.

    6. Improve the UI by giving users more details on what searches they can do and how to better use the results.

    7. Create End to end tests to validate full functionality and prevent regressions.

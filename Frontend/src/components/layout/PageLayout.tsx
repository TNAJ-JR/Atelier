import type { ReactNode } from "react";
import { Link } from "react-router";
import styles from "./PageLayout.module.scss";



export function PageHeader({ title, overline, back, action }: {
  title: string; overline?: string; back?: { to: string; label: string }; action?: ReactNode;
}) {
  return (
    <header className={styles.header}>
      <div>
        {back && <Link className={styles.back} to={back.to}>← {back.label}</Link>}
        {overline && <p className="t-overline text-tertiary">{overline}</p>}
        <h1 className="t-h1">{title}</h1>
      </div>
      {action}
    </header>
  );
}

export function EmptyTable({ columns, label }: { columns: string[]; label: string }) {
  return (
    <div className={styles.tableWrapper}>
      <table className={styles.table}>
        <caption className="visually-hidden">{label}</caption>
        <thead><tr>{columns.map((column) => <th key={column} scope="col">{column}</th>)}</tr></thead>
        <tbody />
      </table>
    </div>
  );
}


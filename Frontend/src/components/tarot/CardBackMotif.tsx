// A small inline SVG sun/star motif for the custom card back (no image asset).
export default function CardBackMotif({ color }: { color: string }) {
  return (
    <svg viewBox="0 0 100 100" width="62%" height="62%" aria-hidden="true">
      <g fill="none" stroke={color} strokeWidth="1.5">
        {Array.from({ length: 12 }).map((_, i) => {
          const a = (i * 30 * Math.PI) / 180;
          const x1 = 50 + Math.cos(a) * 22;
          const y1 = 50 + Math.sin(a) * 22;
          const x2 = 50 + Math.cos(a) * 30;
          const y2 = 50 + Math.sin(a) * 30;
          return <line key={i} x1={x1} y1={y1} x2={x2} y2={y2} />;
        })}
      </g>
      <circle cx="50" cy="50" r="16" fill="none" stroke={color} strokeWidth="1.5" />
      <circle cx="50" cy="50" r="6" fill={color} />
    </svg>
  );
}

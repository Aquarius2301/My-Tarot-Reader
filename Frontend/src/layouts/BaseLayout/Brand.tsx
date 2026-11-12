import { APP_NAME, type Palette } from "@/constants";

type BrandProps = {
  showText: boolean;
  isMobile: boolean;
  palette: Palette;
};

export function Brand({ showText, isMobile, palette }: BrandProps) {
  return (
    <div
      className={`layout-brand${showText ? "" : " layout-brand--collapsed"}`}
      style={{ padding: isMobile ? 0 : undefined }}
    >
      <img className="layout-brand__logo" src="/logo.png" alt={APP_NAME} />
      {showText && (
        <span className="layout-brand__text" style={{ color: palette.primary }}>
          {APP_NAME}
        </span>
      )}
    </div>
  );
}

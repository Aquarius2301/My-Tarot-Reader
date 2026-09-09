import type { CSSProperties, ReactNode } from "react";
import { Tag, theme } from "antd";

export interface OrientationTagProps {
  /** true = reversed (volcano), false = upright (purple). */
  isReversed: boolean;
  /** The localized orientation label (e.g. "Xuôi" / "Ngược"). */
  children: ReactNode;
  /** Extra style merged onto the tag (e.g. padding/margin/font weight). */
  style?: CSSProperties;
}

/** A tag distinguishing a reversed (volcano) vs upright (purple) tarot card,
 * using the shared color mapping and token-based radius/border. */
export default function OrientationTag({
  isReversed,
  children,
  style,
}: OrientationTagProps) {
  const { token } = theme.useToken();

  return (
    <Tag
      color={isReversed ? "volcano" : "purple"}
      style={{
        borderRadius: token.borderRadiusSM,
        border: "none",
        ...style,
      }}
    >
      {children}
    </Tag>
  );
}

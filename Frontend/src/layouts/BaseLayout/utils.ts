import type { LayoutMenuItem } from "@/constants";

export type ActiveMenu = {
  key: string;
  openKeys: string[];
};

export const findActiveMenu = (
  items: LayoutMenuItem[],
  pathname: string,
  parentKeys: string[] = [],
): ActiveMenu | null => {
  let best: ActiveMenu | null = null;

  for (const item of items) {
    const isActive =
      pathname === item.key || pathname.startsWith(`${item.key}/`);
    const hasChildren = Boolean(item.children?.length);

    if (isActive) {
      const candidate: ActiveMenu = {
        key: item.key,
        openKeys: hasChildren ? [...parentKeys, item.key] : parentKeys,
      };
      if (!best || candidate.key.length > best.key.length) {
        best = candidate;
      }
    }

    if (item.children?.length) {
      const childBest = findActiveMenu(item.children, pathname, [
        ...parentKeys,
        item.key,
      ]);
      if (childBest && (!best || childBest.key.length > best.key.length)) {
        best = childBest;
      }
    }
  }

  return best;
};

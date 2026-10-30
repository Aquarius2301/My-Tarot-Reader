/** A shop package as shown to admins, including disabled packages. */
export interface GetAllShopPackagesItem {
  id: string;
  code: string;
  redCoins: number;
  whiteCoins: number;
  oldPrice: number;
  price: number;
  isEnabled: boolean;
}

/** Result of listing every shop package for admins. */
export interface GetAllShopPackagesResult {
  items: GetAllShopPackagesItem[];
}

/** Request to create a shop package. */
export interface CreateShopPackageRequest {
  code: string;
  redCoins: number;
  whiteCoins: number;
  oldPrice: number;
  price: number;
  isEnabled: boolean;
}

/** Request to update an existing shop package. */
export interface UpdateShopPackageRequest {
  code: string;
  redCoins: number;
  whiteCoins: number;
  oldPrice: number;
  price: number;
  isEnabled: boolean;
}

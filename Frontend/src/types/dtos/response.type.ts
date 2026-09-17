/**
 * Generic API response envelope.
 * @template T - The type of the data payload.
 */
export interface ApiResponse<T> {
  success: boolean;
  message: null | string;
  data: T | null;
}

/**
 * Represents a validation error for a specific field in the API request.
 * @property {string} key - The field name that caused the validation error.
 * @property {string} value - The error message associated with the field.
 */
export interface ValidationError {
  key: string;
  value: string;
}

/**
 * Represents an API error response that contains validation errors.
 * Extends the generic ApiResponse interface with a data payload of type ValidationError[].
 * @extends ApiResponse<ValidationError[]>
 */
export interface ApiErrorResponse extends ApiResponse<ValidationError[]> {}

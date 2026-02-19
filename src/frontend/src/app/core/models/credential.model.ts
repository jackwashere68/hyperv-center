export interface Credential {
  id: string;
  name: string;
  username: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCredentialRequest {
  name: string;
  username: string;
  password: string;
}

export interface UpdateCredentialRequest {
  id: string;
  name: string;
  username: string;
  password: string | null;
}

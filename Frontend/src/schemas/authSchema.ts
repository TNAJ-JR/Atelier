import * as yup from "yup";
import type { LoginCredentials, LoginResponse, User } from "../types/authTypes";

export const LoginSchema: yup.ObjectSchema<LoginCredentials> = yup.object({
  email: yup
    .string()
    .email("Veuillez entrer une adresse email valide")
    .required("L'adresse email est requise"),
  password: yup
    .string()
    .min(6, "Le mot de passe doit contenir au moins 6 caractères")
    .required("Le mot de passe est requis"),
});

export const UserSchema: yup.ObjectSchema<User> = yup.object({
  id: yup.string().required(),
  name: yup.string().required(),
  email: yup.string().email().required(),
}).required();

export const LoginResponseSchema: yup.ObjectSchema<LoginResponse> = yup.object({
  token: yup.string().required(),
  user: UserSchema,
}).required();

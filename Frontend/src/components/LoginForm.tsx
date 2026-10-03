import { useForm } from "react-hook-form";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

type FormData = {
  email: string;
  password: string;
};
const schema: yup.ObjectSchema<FormData> = yup.object({
  email: yup
    .string()
    .email("L'adresse email est invalide")
    .required("Email requis"),
  password: yup
    .string()
    .required("Mot de passe requis")
    .min(6, "Le mot de passe doit contenir au moins 6 caractères")
    .matches(
      /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/,
      "Le mot de passe doit contenir au moins une lettre majuscule, une lettre minuscule, un chiffre et un caractère spécial",
    ),
});

function LoginForm() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormData>({
    criteriaMode: "all",
    resolver: yupResolver(schema),
  });

  return (
    <form onSubmit={handleSubmit((data) => console.log(data))}>
      <label htmlFor="email">Email</label>
      <input {...register("email")} type="email" />
      {errors.email && <p>{errors.email.message}</p>}

      <label htmlFor="password">Mot de passe</label>
      <input
        {...register("password")}
        type="password"
        autoComplete="current-password"
        required
      />
      {errors.password && <p>{errors.password.message}</p>}
      <button type="submit">Se connecter</button>
    </form>
  );
}
export default LoginForm;

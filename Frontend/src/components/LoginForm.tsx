import { useForm } from "react-hook-form";
function LoginForm() {
  const { register, handleSubmit } = useForm();
  return (
    <form>
      <label htmlFor="email">Email</label>
      <input {...register("email")} type="email" />

      <label htmlFor="password">Mot de passe</label>
      <input
        {...register("password")}
        type="password"
        autoComplete="current-password"
        required
      />

      <button type="submit">Se connecter</button>
    </form>
  );
}
export default LoginForm;

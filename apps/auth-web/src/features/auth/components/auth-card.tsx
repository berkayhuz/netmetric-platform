import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@netmetric/ui";

type AuthCardProps = {
  title: string;
  description: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
};

export function AuthCard({ title, description, children, footer }: AuthCardProps) {
  return (
    <Card className="w-full border-none ring-0 shadow-none bg-white rounded-2xl p-2 lg:p-8 lg:bg-transparent lg:rounded-none space-y-6">
      <CardHeader className="space-y-2 text-center">
        <CardTitle className="text-balance text-[20px] lg:text-[48px] font-semibold text-[#09090B] leading-tight lg:leading-[61px] tracking-[-0.48px] text-center">
          {title}
        </CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent className="max-w-md w-full mx-auto">{children}</CardContent>
      {footer ? (
        <CardFooter className="border-none bg-transparent p-0 -mt-4">{footer}</CardFooter>
      ) : null}
    </Card>
  );
}

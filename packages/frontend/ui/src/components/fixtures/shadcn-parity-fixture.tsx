"use client";

import { CheckCircle2, CreditCard, Loader2, Search } from "lucide-react";

import { Badge } from "../data-display/badge";
import { Alert, AlertDescription, AlertTitle } from "../feedback/alert";
import { InputGroup, InputGroupAddon, InputGroupInput } from "../forms/input-group";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../layout/card";
import { Separator } from "../layout/separator";
import { Skeleton } from "../layout/skeleton";
import { Tabs, TabsList, TabsTrigger } from "../navigation/tabs";
import { Button } from "../primitives/button";
import { Checkbox } from "../primitives/checkbox";
import { Input } from "../primitives/input";
import { Label } from "../primitives/label";
import { Textarea } from "../primitives/textarea";

export function ShadcnParityFixture() {
  return (
    <div className="grid gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CreditCard className="size-4" />
            Payment Method
          </CardTitle>
          <CardDescription>Add a new card to your account.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Input placeholder="Name" />
          <Input placeholder="Card Number" />
          <div className="grid grid-cols-3 gap-2">
            <Input placeholder="MM" />
            <Input placeholder="YY" />
            <Input placeholder="CVC" />
          </div>
          <Button className="w-full">Continue</Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Team</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex gap-2">
            <Badge>Admin</Badge>
            <Badge variant="secondary">Owner</Badge>
            <Badge variant="outline">Viewer</Badge>
          </div>
          <InputGroup>
            <InputGroupAddon>
              <Search className="size-4" />
            </InputGroupAddon>
            <InputGroupInput placeholder="Search members" />
          </InputGroup>
          <InputGroup>
            <InputGroupAddon>https://</InputGroupAddon>
            <InputGroupInput placeholder="project-url" />
          </InputGroup>
          <Textarea placeholder="Leave a message" />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Two-factor</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center gap-2">
            <Checkbox id="two-factor" />
            <Label htmlFor="two-factor">Require two-factor authentication</Label>
          </div>
          <Tabs defaultValue="sms">
            <TabsList>
              <TabsTrigger value="sms">SMS</TabsTrigger>
              <TabsTrigger value="app">App</TabsTrigger>
              <TabsTrigger value="email">Email</TabsTrigger>
            </TabsList>
          </Tabs>
          <div className="flex gap-2">
            <Button size="sm">Back</Button>
            <Button size="sm" variant="secondary">
              Next
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Compute Environments</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert>
            <CheckCircle2 className="size-4" />
            <AlertTitle>Verified</AlertTitle>
            <AlertDescription>All checks passed.</AlertDescription>
          </Alert>
          <div className="grid gap-3">
            <div className="rounded-md border p-3">
              <div className="text-sm font-medium">Production</div>
              <div className="text-muted-foreground text-sm">4 vCPU / 8 GB</div>
            </div>
            <div className="rounded-md border p-3">
              <div className="text-sm font-medium">Staging</div>
              <div className="text-muted-foreground text-sm">2 vCPU / 4 GB</div>
            </div>
          </div>
          <Separator />
          <div className="rounded-md border p-3 space-y-2">
            <div className="flex items-center gap-2 text-sm">
              <Loader2 className="size-4 animate-spin" />
              Loading
            </div>
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-4 w-1/2" />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
